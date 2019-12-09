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
        [Column("Username")]
        public string Username { get; set; }

        [StringLength(32)]
        [Column("Password")]
        public string Password { get; set; }

        [MaxLength(8)]
        [Column("Kind")]
        public string Kind { get; set; }

        public string Authorization { get; set; }

        public User() {
            Username = null;
            Password = null;
            Kind = null;
            Authorization = null;
        }

        public User(string username, string password, string kind) {
            Username = username;
            Password = password;
            Kind = kind;
            Authorization = null;
        }

        public User(User user) {
            Username = user.Username;
            Password = user.Password;
            Kind = user.Kind;
            Authorization = null;
        }

        public void Encryption() => Password = Md5Hash(this.Password);

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
