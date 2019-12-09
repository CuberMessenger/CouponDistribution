using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CouponDistribution.DataModel {
    //表示用户优惠券的类，没有了amount和left，在判断用户是否重复领取同一优惠券时需要使用
    public class CouponOfCustomer {
        //自增id作为主键
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [StringLength(60)]
        public string Name { get; set; }

        [StringLength(60)]
        public string Description { get; set; }

        public int Stock { get; set; }

        [StringLength(20)]
        public string Username { get; set; }

        public User User { get; set; }

        public CouponOfCustomer(string username, string name, int stock, string description) {
            Username = username;
            Name = name;
            Stock = stock;
            Description = description;
        }

        public CouponOfCustomer() {
            Username = "";
            Name = "";
            Stock = 0;
            Description = "";
        }
    }
}
