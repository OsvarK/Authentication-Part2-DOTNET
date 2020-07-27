using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Models
{
    public class Auth_LoginUserModel
    {
        public string emailOrUsername { get; set; }
        public string password { get; set; }
    }


}
