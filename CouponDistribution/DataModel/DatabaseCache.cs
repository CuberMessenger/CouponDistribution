using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CouponDistribution.DataModel {
    public sealed class DatabaseCache {
        public static readonly DatabaseCache Instance = new DatabaseCache();

        private DatabaseContext Context;

        public Dictionary<string, User> Users { get; set; }

        public Dictionary<string, HashSet<CouponOfSaler>> CouponsOfSaler { get; set; }

        public Dictionary<string, HashSet<CouponOfCustomer>> CouponsOfCustomer { get; set; }

        private DatabaseCache() {

        }


    }
}
