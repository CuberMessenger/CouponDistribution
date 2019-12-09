using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    public class CouponArgumentsBase {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Stock { get; set; }

        public CouponArgumentsBase() {
            Name = null;
            Description = null;
            Stock = 0;
        }

        public CouponArgumentsBase(string name, string description, int stock) {
            Name = name;
            Description = description;
            Stock = stock;
        }

        public CouponArgumentsBase(CouponArgumentsBase couponArgumentsBase) {
            Name = couponArgumentsBase.Name;
            Description = couponArgumentsBase.Description;
            Stock = couponArgumentsBase.Stock;
        }
    }
}
