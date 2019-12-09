using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于查看商家优惠券时进行输出
    public class GetSalerCouponArguments : CouponArgumentsBase {
        public int Amount { get; set; }
        public int Left { get; set; }

        public GetSalerCouponArguments() : base() {
            Amount = 0;
            Left = 0;
        }

        public GetSalerCouponArguments(string name, string description, int stock, int amount, int left) : base(name, description, stock) {
            Amount = amount;
            Left = left;
        }

        public GetSalerCouponArguments(GetSalerCouponArguments getSalerCouponArguments) : base(getSalerCouponArguments as CouponArgumentsBase) {
            Amount = getSalerCouponArguments.Amount;
            Left = getSalerCouponArguments.Left;
        }
    }
}
