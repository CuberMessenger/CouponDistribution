using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using CouponDistribution.DataModel;

namespace CouponDistribution.Controllers {
    //除用户登录之外的所有api的实现
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private DataContext Context;

        //在构造函数时载入数据库
        public UsersController(DataContext _context) {
            Context = _context;
        }

        //辅助，显示所有用户
        [HttpGet]
        public IActionResult Get() {
            return Ok(Context.Users.ToList());
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        //创建用户
        public IActionResult Post([FromBody]User user) {

            if (user.username == null || user.username.Length == 0) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The username can not be empty!" } });
            }
            else if (user.username.Length > 20) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The length of username can not be larger than 20!" } });
            }
            var _user = Context.Users.FirstOrDefault(r => r.username == user.username);
            if (_user != null) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The username had already been registered." } });
            }

            if (user.password == null || user.password.Length == 0) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The password can not be empty!" } });
            }

            if (user.kind == null || user.kind.Length == 0) {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The type of user can not be empty!" } });
            }
            else if (user.kind != "saler" && user.kind != "customer") {
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The type of user can be either saler or customer." } });
            }

            user.Encryption();
            Context.Users.Add(user);
            Context.SaveChanges();
            return Created($"api/users/{user.username}", null);
        }


        //辅助，查看某一用户
        [HttpGet("{username}")]
        public IActionResult Get(string username) {
            var _user = Context.Users.FirstOrDefault(r => r.username == username);
            if (_user == null)
                return NotFound();
            return Ok(_user);
        }

        //商家新建优惠券
        [HttpPost("{username}/coupons")]
        public IActionResult SetCoupon(string username, [FromHeader] string Authorization, [FromBody]input1 input) {
            var _user = Context.Users.FirstOrDefault(r => r.username == username);
            if (_user == null || _user.auth != Authorization)
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });
            else if (_user.kind == "customer")
                return BadRequest(new Dictionary<string, string> { { "errMsg", "Only salers can set new coupons." } });

            if (input.name == null || input.name.Length == 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The name of coupon cannot be empty." } });
            else if (Context.Coupons.Include(r => r.User).FirstOrDefault(r => r.name == input.name) != null)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The coupon does exist." } });

            if (input.amount <= 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The amount of coupon should be larger than 0." } });
            if (input.stock <= 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The stock of coupon should be larger than 0." } });

            var _coupon = new Coupon(username, input.name, input.stock, input.description, input.amount);
            Context.Coupons.Add(_coupon);
            Context.SaveChanges();
            return Created($"api/users/{username}/coupons/{_coupon.name}", null);
        }

        //查看优惠券
        [HttpGet("{username}/coupons")]
        public IActionResult CheckCoupon(string username, [FromHeader] string Authorization, int page) {
            if (page <= 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The parameter page should be larger than 0." } });
            page -= 1;

            var _user = Context.Users.FirstOrDefault(r => r.auth == Authorization);
            if (_user == null)
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });

            if (_user.username == username && _user.kind == "customer") {
                var coupon0 = Context.Coupons2.Include(r => r.User).Where(r => r.username == username).ToArray();
                if (coupon0.Length <= page * 20)
                    return NoContent();
                else {
                    int len0 = coupon0.Length;

                    var arr0 = new List<input3>();
                    for (int i = page * 20; i < (page + 1) * 20 && i < len0; i++) {
                        var input3 = new input3(coupon0[i].name, coupon0[i].stock, coupon0[i].description);
                        arr0.Add(input3);
                    }

                    arr0.ToArray();
                    Dictionary<string, List<input3>> result0 = new Dictionary<string, List<input3>>
                    {
                        { "data", arr0 }
                    };
                    return Ok(result0);
                }
            }

            var _user1 = Context.Users.FirstOrDefault(r => r.username == username);
            if (_user1 == null)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for does not exist." } });
            else if (_user1.kind == "customer")
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for is a customer instead of a saler." } });

            var coupon = Context.Coupons.Include(r => r.User).Where(r => r.username == username).ToArray();
            int len = coupon.Length;

            if (len <= page * 20)
                return NoContent();

            var arr = new List<input2>();
            for (int i = page * 20; i < (page + 1) * 20 && i < len; i++) {
                var input2 = new input2(coupon[i].name, coupon[i].amount, coupon[i].left, coupon[i].stock, coupon[i].description);
                arr.Add(input2);
            }

            arr.ToArray();
            Dictionary<string, List<input2>> result = new Dictionary<string, List<input2>>
            {
                { "data", arr }
            };
            return Ok(result);

        }

        //用户获取优惠券
        [HttpPatch("{username}/coupons/{name}")]
        public IActionResult GetCoupon(string username, string name, [FromHeader] string Authorization) {
            var _user = Context.Users.FirstOrDefault(r => r.auth == Authorization);
            if (_user == null)
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Authorization failed." } });
            else if (_user.kind == "saler")
                return BadRequest(new Dictionary<string, string> { { "errMsg", "Only costomers can get coupons." } });

            var _user1 = Context.Users.FirstOrDefault(r => r.username == username);
            if (_user1 == null)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for does not exist." } });
            else if (_user1.kind == "customer")
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The user you search for is a customer instead of a saler." } });

            var _coupon = Context.Coupons.Include(r => r.User).Where(r => r.username == _user1.username).FirstOrDefault(r => r.name == name);
            if (_coupon == null)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "According to the username, the coupon you search for does not exist." } });
            else if (_coupon.left == 0)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "The number of this coupon is 0." } });

            var _coupon1 = Context.Coupons2.Include(r => r.User).Where(r => r.username == _user.username).FirstOrDefault(r => r.name == name);
            if (_coupon1 != null)
                return BadRequest(new Dictionary<string, string> { { "errMsg", "You have owned this coupon." } });

            _coupon.left -= 1;
            Context.Coupons.Update(_coupon);

            var _coupon2 = new Coupon2(_user.username, name, _coupon.stock, _coupon.description);
            Context.Coupons2.Add(_coupon2);

            Context.SaveChanges();

            return Created("", null);
        }

    }
}
