using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    public sealed class DatabaseCache {
        public static readonly DatabaseCache Instance = new DatabaseCache();

        private DatabaseContext Context = null;

        public Dictionary<string, User> Users { get; set; }

        public Dictionary<string, User> HashToUser { get; set; }

        public Dictionary<string, Dictionary<string, CouponOfSaler>> CouponsOfSaler { get; set; }

        public Dictionary<string, Dictionary<string, CouponOfCustomer>> CouponsOfCustomer { get; set; }

        private DatabaseCache() {
        }

        public void Initiate(DatabaseContext context) {
            Context = context;

            var couponsOfSaler = Context.CouponsOfSaler.ToList();
            foreach (var coupon in couponsOfSaler) {
                CouponsOfSaler[coupon.Username][coupon.Name] = coupon;
            }

            var couponsOfCustomer = Context.CouponsOfCustomer.ToList();
            foreach (var coupon in couponsOfCustomer) {
                CouponsOfCustomer[coupon.Username][coupon.Name] = coupon;
            }

            var users = Context.Users.ToList();
            foreach (var user in users) {
                Users[user.Username] = user;
                if (!string.IsNullOrEmpty(user.Authorization)) {
                    HashToUser[user.Authorization] = user;
                }
                if (user.Kind == "saler") {
                    if (!CouponsOfSaler.ContainsKey(user.Username)) {
                        CouponsOfSaler[user.Username] = new Dictionary<string, CouponOfSaler>();
                    }
                }
                if (user.Kind == "customer") {
                    if (!CouponsOfCustomer.ContainsKey(user.Username)) {
                        CouponsOfCustomer[user.Username] = new Dictionary<string, CouponOfCustomer>();
                    }
                }
            }
        }

        public async static void UpdateOperation(ThreadStart operation) => new Thread(operation).Start();
    }
}
