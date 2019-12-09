using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于查看用户优惠券时进行输出
    public class GetCustomerCouponArguments : CouponArgumentsBase {
        public GetCustomerCouponArguments() : base() {
        }

        public GetCustomerCouponArguments(string name, string description, int stock) : base(name, description, stock) {
        }

        public GetCustomerCouponArguments(GetCustomerCouponArguments getCustomerCouponArguments) : base(getCustomerCouponArguments as CouponArgumentsBase) {
        }
    }
}
