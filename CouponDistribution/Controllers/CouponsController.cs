using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouponDistribution.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CouponDistribution.Controllers {
    //辅助，显示商家持有的优惠券的情况
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsController : ControllerBase {
        private DataContext Context;

        public CouponsController(DataContext _context) {
            Context = _context;
        }

        [HttpGet]
        public IActionResult Get() {
            return Ok(Context.Coupons.ToList());
        }
    }
}