using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    //辅助，用于设置优惠券时读取输入
    public class input1 {
        public string name { get; set; }
        public int amount { get; set; }
        public string description { get; set; }
        public int stock { get; set; }

        public input1(string name, int amount, string description, int stock) {
            this.name = name;
            this.amount = amount;
            this.description = description;
            this.stock = stock;
        }

        public input1(input1 input) {
            this.name = input.name;
            this.amount = input.amount;
            this.description = input.description;
            this.stock = input.stock;
        }

        public input1() {
            this.name = null;
            this.amount = 0;
            this.description = null;
            this.stock = 0;
        }
    }
}
