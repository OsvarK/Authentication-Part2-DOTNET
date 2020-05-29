using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationAPI.Security
{
    public class Hash
    {

        private readonly IConfiguration _configuration;
        private string _secretsalt;

        public Hash()
        {
            _configuration = new ConfigContex().configuration;
            _secretsalt = _configuration["Security:Salt"];
        }


        public string HashPassword(string unHashedPassword)
        {
            // Salt
            byte[] salt = Encoding.ASCII.GetBytes(_secretsalt);

            // Hash password
            var pbkdf2 = new Rfc2898DeriveBytes(unHashedPassword, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);

            // Combine salt and Password
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            // Stringyfy password and return it
            string hasedPassword = Convert.ToBase64String(hashBytes);
            return hasedPassword;
        }
    }
}
