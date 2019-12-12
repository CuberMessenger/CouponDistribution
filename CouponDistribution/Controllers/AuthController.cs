using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CouponDistribution.DataModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CouponDistribution.Controllers {
    //用户登录
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private DatabaseContext Context;

        public AuthController(DatabaseContext context) => Context = context;

        [HttpPost]
        public IActionResult Login([FromBody]User user) {
            if (string.IsNullOrEmpty(user.Username)) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "The username can not be empty!" } });
            }
            if (string.IsNullOrEmpty(user.Password)) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "The password can not be empty!" } });
            }

            user.Encryption();
            User _user;
            bool flag = DatabaseCache.Instance.Users.TryGetValue(user.Username, out _user);
            if (!flag) {
                _user = null;
            }
            //var _user = Context.Users.FirstOrDefault(r => r.Username == user.Username);
            if (_user == null) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Can not find the username." } });
            }
            else if (_user.Password != user.Password) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Wrong password." } });
            }
            else {
                //Random random = new Random();
                //_user.auth = _user.Md5Hash(_user.password + random.Next().ToString());

                _user.Authorization = _user.Md5Hash(_user.Username + _user.Password + _user.Username);
                DatabaseCache.Instance.Users[_user.Username].Authorization = _user.Authorization;
                DatabaseCache.Instance.HashToUser[_user.Authorization] = _user;

                new Thread(() => {
                    var context = new DatabaseContext(new DbContextOptionsBuilder<DatabaseContext>().UseSqlite("Filename=./user.db").Options);
                    context.Users.Update(_user);
                    context.SaveChanges();
                }).Start();

                //Context.Users.Update(_user);
                //Context.SaveChanges();

                //将token写入返回的头部
                Response.Headers.Append("Authorization", _user.Authorization);
                return Ok(new Dictionary<string, string> { { "kind", _user.Kind }, { "errMsg", "" } });
            }
        }
    }
}
