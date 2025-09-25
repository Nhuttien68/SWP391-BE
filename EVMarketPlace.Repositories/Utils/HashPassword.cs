using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Repositories.Utils
{
    public class HashPassword
    {
        public static string HashPasswordSHA256(string rawPassword)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
            return Convert.ToBase64String(bytes);
        }
    }
}
