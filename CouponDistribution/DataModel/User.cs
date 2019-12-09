using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CouponDistribution.DataModel {
    public class User {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(20)]
        [Column("username")]
        public string username { get; set; }

        [StringLength(32)]
        [Column("password")]
        public string password { get; set; }

        [MaxLength(8)]
        [Column("kind")]
        public string kind { get; set; }

        public string auth { get; set; }
        public User(string username, string password, string kind) {
            this.username = username;
            this.password = password;
            this.kind = kind;
            this.auth = null;
        }

        public User() {
            this.username = null;
            this.password = null;
            this.kind = null;
            this.auth = null;
        }

        public User(User user) {
            this.username = user.username;
            this.password = user.password;
            this.kind = user.kind;
            this.auth = null;
        }

        public void Encryption() {
            this.password = Md5Hash(this.password);
        }

        // 32位MD5加密
        public string Md5Hash(string input) {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++) {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
