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

        public Tuple<bool, string> Auth_UserModelValidationSignup(Auth_UserModel user)
        {
            string[] stringsToValidate = 
            {
                user.Firstname,
                user.Lastname,
                user.Username,
                user.Email,
                user.Password
            };

            return InputChecker(stringsToValidate);
        }
        

        public Tuple<bool, string> Auth_UserModelValidationLogin(Auth_UserModel user)
        {
            string[] stringsToValidate =
            {
                user.Username,
                user.Password
            };

            return InputChecker(stringsToValidate);
        }

        // Custom input Checker
        Tuple<bool, string> InputChecker(string[] input)
        {
            foreach (string data in input)
            {
                if(data == null)
                    return Tuple.Create(false, "Input field must not be empty.");
                if (!Regex.IsMatch(data, @"^[a-zA-Z0-9_@.,#*?!]+$"))
                    return Tuple.Create(false, "Invalid symbols in input field.");
                if (string.IsNullOrWhiteSpace(data))
                    return Tuple.Create(false, "White spaces or empty fields are not allowed.");
            }
            return Tuple.Create(true, string.Empty);
        }
    }
}