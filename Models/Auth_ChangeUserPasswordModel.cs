using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Models
{
    public class Auth_ChangeUserPasswordModel
    {
        public string currentPassword { get; set; }
        public string newPassword { get; set; }
    }
}
