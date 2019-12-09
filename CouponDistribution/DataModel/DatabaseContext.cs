using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CouponDistribution.DataModel {
    public class DatabaseContext : DbContext {
        //数据库，由三张表组成
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) {
        }

        //用户表
        public DbSet<User> Users { get; set; }

        //商家优惠券表，该表与用户表是多对一的关系
        public DbSet<CouponOfSaler> CouponsOfSaler { get; set; }

        //用户优惠券表，该表与用户表是多对一的关系
        public DbSet<CouponOfCustomer> CouponsOfCustomer { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            //配置表模型信息
            var couponOfSaler = modelBuilder.Entity<CouponOfSaler>();
            //配置关系,外键是username，不许为空
            couponOfSaler.HasOne(r => r.User).WithMany().HasForeignKey(r => r.Username).IsRequired();

            var couponOfCustomer = modelBuilder.Entity<CouponOfCustomer>();
            couponOfCustomer.HasOne(r => r.User).WithMany().HasForeignKey(r => r.Username).IsRequired();
        }
    }
}
