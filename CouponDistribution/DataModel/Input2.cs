using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于查看商家优惠券时进行输出
    public class input2 {
        public string name { get; set; }
        public int amount { get; set; }
        public int left { get; set; }
        public int stock { get; set; }
        public string description { get; set; }


        public input2(string name, int amount, int left, int stock, string description) {
            this.name = name;
            this.stock = stock;
            this.description = description;
            this.amount = amount;
            this.left = left;
        }

        public input2() {
            this.name = "";
            this.stock = 0;
            this.description = "";
            this.amount = 0;
            this.left = 0;
        }
    }
}
