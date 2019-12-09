using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouponDistribution.DataModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CouponDistribution.Controllers {
    //用户登录
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase {
        private DatabaseContext Context;

        public AuthorizationController(DatabaseContext _context) {
            Context = _context;
        }

        [HttpPost]

        public IActionResult Login([FromBody]User user) {
            if (user.username == null || user.username.Length == 0) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "The username can not be empty!" } });
            }
            if (user.password == null || user.password.Length == 0) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "The password can not be empty!" } });
            }

            user.Encryption();
            var _user = Context.Users.FirstOrDefault(r => r.username == user.username);
            if (_user == null) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Can not find the username." } });
            }
            else if (_user.password != user.password) {
                return Unauthorized(new Dictionary<string, string> { { "errMsg", "Wrong password." } });
            }
            else {
                //Random random = new Random();
                //_user.auth = _user.Md5Hash(_user.password + random.Next().ToString());

                _user.auth = _user.Md5Hash(_user.username + _user.password + _user.username);
                Context.Users.Update(_user);
                Context.SaveChanges();

                //将token写入返回的头部
                Response.Headers.Append("Authorization", _user.auth);

                return Ok(new Dictionary<string, string> { { "kind", _user.kind } });
            }
        }
    }
}
