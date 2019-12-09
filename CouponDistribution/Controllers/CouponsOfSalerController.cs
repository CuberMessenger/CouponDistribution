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
    public class CouponsOfSalerController : ControllerBase {
        private DatabaseContext Context;

        public CouponsOfSalerController(DatabaseContext context) => Context = context;

        [HttpGet]
        public IActionResult Get() => Ok(Context.CouponsOfSaler.ToList());
    }
}