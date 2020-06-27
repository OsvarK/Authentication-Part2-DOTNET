using System;

namespace AuthenticationAPI.Models
{
    public class Auth_UserModel
    {      
        // UserID is also going to be the primary key in the table
        public string Username { get; set; }    // Same in database
        public string Firstname { get; set; }   // Same in database
        public string Lastname { get; set; }    // Same in database
        public string Email { get; set; }       // Same in database
        public string Password { get; set; }    // Same in database

        public string ValidatedPassword { get; set; } // not be in database, only in use under edit user action.
    }
}
