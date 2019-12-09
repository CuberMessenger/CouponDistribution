using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于查看用户优惠券时进行输出
    public class input3 {
        public string name { get; set; }
        public int stock { get; set; }
        public string description { get; set; }


        public input3(string name, int stock, string description) {
            this.name = name;
            this.stock = stock;
            this.description = description;
        }

        public input3() {
            this.name = "";
            this.stock = 0;
            this.description = "";
        }
    }
}
