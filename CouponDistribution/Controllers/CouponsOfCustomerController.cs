using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CouponDistribution.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CouponDistribution.Controllers {
    //辅助，显示用户持有的优惠券的情况
    [Route("api/[controller]")]
    [ApiController]
    public class CouponsOfCustomerController : ControllerBase {
        private DatabaseContext Context;

        public CouponsOfCustomerController(DatabaseContext context) => Context = context;

        [HttpGet]
        public IActionResult Get() => Ok(Context.CouponsOfCustomer.ToList());
    }
}