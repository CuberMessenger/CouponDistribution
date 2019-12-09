using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CouponDistribution.DataModel {
    //表示商家优惠券的类
    public class Coupon {
        [Key]
        [StringLength(60)]
        public string name { get; set; }
        public int amount { get; set; }
        public int left { get; set; }
        public int stock { get; set; }
        [StringLength(60)]
        public string description { get; set; }
        [StringLength(20)]

        public string username { get; set; }

        public User User { get; set; }

        public Coupon(string username, string name, int stock, string description, int amount) {
            this.username = username;
            this.name = name;
            this.stock = stock;
            this.description = description;
            this.amount = amount;
            this.left = amount;
        }

        public Coupon() {
            this.username = "";
            this.name = "";
            this.stock = 0;
            this.description = "";
            this.amount = 0;
            this.left = 0;
        }

    }
}
