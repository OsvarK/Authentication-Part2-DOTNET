using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Models
{
    public class Auth_RegisterModel
    {
        // UserID is also going to be the primary key in the table
        public string Username { get; set; }    // Same in database
        public string Firstname { get; set; }   // Same in database
        public string Lastname { get; set; }    // Same in database
        public string Email { get; set; }       // Same in database
        public string Password { get; set; }    // Same in database

        public void EditModelToRegisterModel(Auth_EditProfileModel auth_EditProfileModel)
        {
            Username = auth_EditProfileModel.Username;
            Email = auth_EditProfileModel.Email;
            Firstname = auth_EditProfileModel.Firstname;
            Lastname = auth_EditProfileModel.Lastname;
            Password = auth_EditProfileModel.Password;
        }
    }
}
