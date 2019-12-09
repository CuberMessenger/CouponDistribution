using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CouponDistribution.DataModel {
    //表示用户优惠券的类，没有了amount和left，在判断用户是否重复领取同一优惠券时需要使用
    public class Coupon2 {
        //自增id作为主键
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [StringLength(60)]
        public string name { get; set; }
        public int stock { get; set; }
        [StringLength(60)]
        public string description { get; set; }
        [StringLength(20)]

        public string username { get; set; }

        public User User { get; set; }


        public Coupon2(string username, string name, int stock, string description) {
            this.username = username;
            this.name = name;
            this.stock = stock;
            this.description = description;
        }

        public Coupon2() {
            this.username = "";
            this.name = "";
            this.stock = 0;
            this.description = "";
        }
    }
}
