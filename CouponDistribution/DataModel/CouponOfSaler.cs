using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CouponDistribution.DataModel {
    //表示商家优惠券的类
    public class CouponOfSaler {
        [Key]
        [StringLength(60)]
        public string Name { get; set; }

        [StringLength(60)]
        public string Description { get; set; }

        public int Amount { get; set; }

        public int Left { get; set; }

        public int Stock { get; set; }

        [StringLength(20)]
        public string Username { get; set; }

        public User User { get; set; }

        public CouponOfSaler(string username, string name, int stock, string description, int amount) {
            Username = username;
            Name = name;
            Stock = stock;
            Description = description;
            Amount = amount;
            Left = amount;
        }

        public CouponOfSaler() {
            Username = "";
            Name = "";
            Stock = 0;
            Description = "";
            Amount = 0;
            Left = 0;
        }
    }
}
