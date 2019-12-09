using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouponDistribution.DataModel {
    public class DatabaseContext : DbContext {
        //数据库，由三张表组成
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options) {
        }

        //用户表
        public DbSet<User> Users { get; set; }

        //商家优惠券表，该表与用户表是多对一的关系
        public DbSet<Coupon> Coupons { get; set; }

        //用户优惠券表，该表与用户表是多对一的关系
        public DbSet<Coupon2> Coupons2 { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            //配置表模型信息
            var coupon1 = modelBuilder.Entity<Coupon>();
            //配置关系,外键是username，不许为空
            coupon1.HasOne(r => r.User).WithMany().HasForeignKey(r => r.username).IsRequired();


            var coupon2 = modelBuilder.Entity<Coupon2>();
            coupon2.HasOne(r => r.User).WithMany().HasForeignKey(r => r.username).IsRequired();

        }
    }
}
