using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using CouponDistribution.DataModel;
using System.Threading;

namespace CouponDistribution.Controllers {
    //除用户登录之外的所有api的实现
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private DatabaseContext Context;

        //在构造函数时载入数据库
        public UsersController(DatabaseContext context) => Context = context;

        //辅助，显示所有用户
        [HttpGet]
        public IActionResult Get() => Ok(DatabaseCache.Instance.Users.Values.ToList());

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //创建用户
        public IActionResult Post([FromBody]User user) {
            if (string.IsNullOrEmpty(user.Username)) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The username can not be empty!" } });
            }
            if (user.Username.Length > 20) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The length of username can not be larger than 20!" } });
            }

            User _user;
            bool flag = DatabaseCache.Instance.Users.TryGetValue(user.Username, out _user);
            if (!flag) {
                _user = null;
            }
            //var _user = Context.Users.FirstOrDefault(r => r.Username == user.Username);
            if (_user != null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The username had already been registered." } });
            }

            if (string.IsNullOrEmpty(user.Password)) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The password can not be empty!" } });
            }

            if (string.IsNullOrEmpty(user.Kind)) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The type of user can not be empty!" } });
            }
            if (user.Kind != "saler" && user.Kind != "customer") {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The type of user can be either saler or customer." } });
            }

            user.Encryption();

            DatabaseCache.Instance.Users[user.Username] = user;
            if (user.Kind == "saler") {
                DatabaseCache.Instance.CouponsOfSaler[user.Username] = new Dictionary<string, CouponOfSaler>();
            }
            if (user.Kind == "customer") {
                DatabaseCache.Instance.CouponsOfCustomer[user.Username] = new Dictionary<string, CouponOfCustomer>();
            }

            DatabaseCache.Instance.DatabaseOperations.Enqueue(() => {
                var context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite("Filename=./user.db").Options);
                context.Users.Add(user);
                context.SaveChanges();
            });
            DatabaseCache.Instance.QueueLock.Release();

            //Context.Users.Add(user);
            //Context.SaveChanges();
            return Created($"api/users/{user.Username}", new Dictionary<string, string> { { "errMsg", "" } });
        }


        //辅助，查看某一用户
        [HttpGet("{username}")]
        public IActionResult Get(string username) {
            User _user;
            bool flag = DatabaseCache.Instance.Users.TryGetValue(username, out _user);
            if (!flag) {
                _user = null;
            }
            if (_user == null)
                return NotFound();
            return Ok(_user);
        }

        //商家新建优惠券
        [HttpPost("{username}/coupons")]
        public IActionResult SetCoupon(string username, [FromHeader]string authorization, [FromBody]NewCouponArguments arg) {
            User _user;
            bool flag = DatabaseCache.Instance.Users.TryGetValue(username, out _user);
            if (!flag) {
                _user = null;
            }
            //var _user = Context.Users.FirstOrDefault(r => r.Username == username);
            if (_user == null || _user.Authorization != authorization) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });
            }
            else if (_user.Kind == "customer") {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "Only salers can set new coupons." } });
            }

            if (string.IsNullOrEmpty(arg.Name)) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The name of coupon cannot be empty." } });
            }
            else {
                CouponOfSaler coupon;
                flag = DatabaseCache.Instance.CouponsOfSaler[username].TryGetValue(arg.Name, out coupon);
                if (flag) {
                    return BadRequest(new Dictionary<string, string> { { "errMsg", "The coupon does exist." } });
                }
            }
            //else if (Context.CouponsOfSaler.Include(r => r.User).FirstOrDefault(r => r.Name == arg.Name) != null) {
            //    return BadRequest(new Dictionary<string, string> { { "errMsg", "The coupon does exist." } });
            //}

            if (arg.Amount <= 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The amount of coupon should be larger than 0." } });
            if (arg.Stock <= 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The stock of coupon should be larger than 0." } });

            var _coupon = new CouponOfSaler(username, arg.Name, arg.Stock, arg.Description, arg.Amount);
            DatabaseCache.Instance.CouponsOfSaler[username][arg.Name] = _coupon;

            DatabaseCache.Instance.DatabaseOperations.Enqueue(() => {
                var context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite("Filename=./user.db").Options);
                context.CouponsOfSaler.Add(_coupon);
                context.SaveChanges();
            });
            DatabaseCache.Instance.QueueLock.Release();

            //Context.CouponsOfSaler.Add(_coupon);
            //Context.SaveChanges();
            return Created($"api/users/{username}/coupons/{_coupon.Name}", new Dictionary<string, string> { { "errMsg", "" } });
        }

        //查看优惠券
        [HttpGet("{username}/coupons")]
        public IActionResult CheckCoupon(string username, [FromHeader]string Authorization, [FromBody]string spage) {
            int _page = int.Parse(spage.Split('\"')[3]);
            if (_page <= 0) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The parameter page should be larger than 0." } });
            }
            _page -= 1;

            User _user;
            bool flag = DatabaseCache.Instance.HashToUser.TryGetValue(Authorization, out _user);
            if (!flag) {
                _user = null;
            }
            //var _user = Context.Users.FirstOrDefault(r => r.Authorization == Authorization);
            if (_user == null) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });
            }

            if (_user.Username == username && _user.Kind == "customer") {
                var coupon0 = DatabaseCache.Instance.CouponsOfCustomer[username].Values.ToArray();

                //var coupon0 = Context.CouponsOfCustomer.Include(r => r.User).Where(r => r.Username == username).ToArray();
                if (coupon0.Length <= _page * 20) {
                    return NoContent();
                }
                else {
                    int len0 = coupon0.Length;

                    var arr0 = new List<GetCustomerCouponArguments>();
                    for (int i = _page * 20; i < (_page + 1) * 20 && i < len0; i++) {
                        var input3 = new GetCustomerCouponArguments(coupon0[i].Name, coupon0[i].Description, coupon0[i].Stock);
                        arr0.Add(input3);
                    }

                    arr0.ToArray();
                    Dictionary<string, List<GetCustomerCouponArguments>> result0 = new Dictionary<string, List<GetCustomerCouponArguments>>
                    {
                        { "data", arr0 }
                    };
                    return Ok(result0);
                }
            }

            User _user1;
            flag = DatabaseCache.Instance.Users.TryGetValue(username, out _user1);
            if (!flag) {
                _user1 = null;
            }
            //var _user1 = Context.Users.FirstOrDefault(r => r.Username == username);
            if (_user1 == null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for does not exist." } });
            }
            else if (_user1.Kind == "customer") {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "The user you search for is a customer instead of a saler." } });
            }

            var coupon = DatabaseCache.Instance.CouponsOfSaler[username].Values.ToArray();
            //var coupon = Context.CouponsOfSaler.Include(r => r.User).Where(r => r.Username == username).ToArray();
            int len = coupon.Length;

            if (len <= _page * 20)
                return NoContent();

            var arr = new List<GetSalerCouponArguments>();
            for (int i = _page * 20; i < (_page + 1) * 20 && i < len; i++) {
                var input2 = new GetSalerCouponArguments(coupon[i].Name, coupon[i].Description, coupon[i].Stock, coupon[i].Amount, coupon[i].Left);
                arr.Add(input2);
            }

            arr.ToArray();
            Dictionary<string, List<GetSalerCouponArguments>> result = new Dictionary<string, List<GetSalerCouponArguments>>
            {
                { "data", arr }
            };
            return Ok(result);
        }

        //用户获取优惠券
        [HttpPatch("{username}/coupons/{name}")]
        public IActionResult GetCoupon(string username, string name, [FromHeader]string Authorization) {
            User _user;
            bool flag = DatabaseCache.Instance.HashToUser.TryGetValue(Authorization, out _user);
            if (!flag) {
                _user = null;
            }
            //var _user = Context.Users.FirstOrDefault(r => r.Authorization == Authorization);
            if (_user == null) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });
            }
            else if (_user.Kind == "saler") {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "Only costomers can get coupons." } });
            }

            User _user1;
            flag = DatabaseCache.Instance.Users.TryGetValue(username, out _user1);
            if (!flag) {
                _user1 = null;
            }
            //var _user1 = Context.Users.FirstOrDefault(r => r.Username == username);
            if (_user1 == null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for does not exist." } });
            }
            else if (_user1.Kind == "customer") {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for is a customer instead of a saler." } });
            }

            CouponOfSaler _coupon;
            flag = DatabaseCache.Instance.CouponsOfSaler[_user1.Username].TryGetValue(name, out _coupon);
            if (!flag) {
                _coupon = null;
            }
            //var _coupon = Context.CouponsOfSaler.Include(r => r.User).Where(r => r.Username == _user1.Username).FirstOrDefault(r => r.Name == name);
            if (_coupon == null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "According to the username, the coupon you search for does not exist." } });
            }
            else if (_coupon.Left == 0) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The number of this coupon is 0." } });
            }

            CouponOfCustomer _coupon1;
            flag = DatabaseCache.Instance.CouponsOfCustomer[_user.Username].TryGetValue(name, out _coupon1);
            if (!flag) {
                _coupon1 = null;
            }
            //var _coupon1 = Context.CouponsOfCustomer.Include(r => r.User).Where(r => r.Username == _user.Username).FirstOrDefault(r => r.Name == name);
            if (_coupon1 != null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "You have owned this coupon." } });
            }

            _coupon.Left -= 1;
            DatabaseCache.Instance.CouponsOfSaler[_user1.Username][name].Left = _coupon.Left;

            var _coupon2 = new CouponOfCustomer(_user.Username, name, _coupon.Stock, _coupon.Description);
            DatabaseCache.Instance.CouponsOfCustomer[_user.Username][name] = _coupon2;

            DatabaseCache.Instance.DatabaseOperations.Enqueue(() => {
                var context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite("Filename=./user.db").Options);
                context.CouponsOfSaler.Update(_coupon);
                context.CouponsOfCustomer.Add(_coupon2);
                context.SaveChanges();
            });
            DatabaseCache.Instance.QueueLock.Release();

            //Context.CouponsOfSaler.Update(_coupon);
            //Context.CouponsOfCustomer.Add(_coupon2);
            //Context.SaveChanges();

            return Created("", null);
        }
    }
}
