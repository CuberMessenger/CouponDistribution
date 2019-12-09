using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于设置优惠券时读取输入
    public class NewCouponArguments : CouponArgumentsBase {
        public int Amount { get; set; }

        public NewCouponArguments() : base() => Amount = 0;

        public NewCouponArguments(string name, string description, int stock, int amount) : base(name, description, stock) => Amount = amount;

        public NewCouponArguments(NewCouponArguments newCouponArguments) : base(newCouponArguments as CouponArgumentsBase) => Amount = newCouponArguments.Amount;
    }
}
