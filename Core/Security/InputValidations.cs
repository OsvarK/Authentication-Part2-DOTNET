using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthenticationAPI.Models;

namespace AuthenticationAPI.Security
{
    public class InputValidations
    {
        // This class is for filtering and is supposed to be easy to modify

        public bool Auth_UserModelValidationSignup(Auth_UserModel user)
        {
            //Check if the data in userModel is safe

            if (InputChecker(user.Username) &&
                InputChecker(user.Firstname) &&
                InputChecker(user.Lastname) &&
                InputChecker(user.Email) &&
                InputChecker(user.Password)) 
            {
                return true;
            } else
            {
                return false;
            }
        }

        public bool Auth_UserModelValidationLogin(Auth_UserModel user)
        {
            //Check if the data in userModel is safe

            if (InputChecker(user.Username) &&
                InputChecker(user.Password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Custom input Checker
        public bool InputChecker(string input)
        {    // if empty return false
            if(input == null)
            {
                return false;
            }
            // Return false if contains anything that isent this symbols a-zA-Z0-9_@.,#*?!
            return Regex.IsMatch(input, @"^[a-zA-Z0-9_@.,#*?!]+$");
        }
    }
}