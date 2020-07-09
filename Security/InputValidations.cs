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
            if(!IsValidEmail(user.Email))
                return Tuple.Create(false, "Email is not a valid email.");

            string[] stringsToValidate = 
            {
                user.Firstname,
                user.Lastname,
                user.Username,
                user.Email,
                user.Password
            };
            Tuple<bool, string> doneGeneralValidation = GeneralInputChecker(stringsToValidate);
            Tuple<bool, string> donePasswordValidation = PasswordInputChecker(user.Password);
            Tuple<bool, string> doneUsernameValidation = UsernameInputChecker(user.Username);
            if (!doneGeneralValidation.Item1)
                return doneGeneralValidation;
            if (!donePasswordValidation.Item1)
                return donePasswordValidation;
            if (!doneUsernameValidation.Item1)
                return doneUsernameValidation;
            return Tuple.Create(true, String.Empty);
        }
        

        public Tuple<bool, string> Auth_UserModelValidationLogin(Auth_UserModel user)
        {
            string[] stringsToValidate =
            {
                user.Username,
                user.Password
            };
            Tuple<bool, string> doneGeneralValidation = GeneralInputChecker(stringsToValidate);
            if (!doneGeneralValidation.Item1)
                return doneGeneralValidation;
            return Tuple.Create(true, String.Empty);
        }

        // Custom input Checkers -------------------------------------------------------------      
        Tuple<bool, string> GeneralInputChecker(string[] input)
        {
            foreach (string data in input)
            {
                if(data == null)
                    return Tuple.Create(false, "Input field must not be empty.");
                if (string.IsNullOrWhiteSpace(data))
                    return Tuple.Create(false, "White spaces or empty fields are not allowed.");
            }
            return Tuple.Create(true, string.Empty);
        }

        Tuple<bool, string> PasswordInputChecker(string input)
        {
            if (input.Length < 8)
                return Tuple.Create(false, "Password needs to be more or equal to 8 characters.");
            if (!input.Any(char.IsLower))
                return Tuple.Create(false, "Password needs atleast one lower case.");
            if (!input.Any(char.IsUpper))
                return Tuple.Create(false, "Password needs atleast one upper case.");
            if (!input.Any(char.IsDigit))
                return Tuple.Create(false, "Password needs atleast one digit.");
            if (AtleastOneSymbol(input))
                return Tuple.Create(false, "Password needs atleast one special character.");
            return Tuple.Create(true, string.Empty);
        }

        Tuple<bool, string> UsernameInputChecker(string input)
        {
            if (input.Length < 5)
                return Tuple.Create(false, "Username needs to be more or equal to 5 characters.");
            if (!AtleastOneSymbol(input))
                return Tuple.Create(false, "Username cannot have any special character.");
            return Tuple.Create(true, string.Empty);
        }
   
        static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        bool AtleastOneSymbol(string input)
        {
            string specialChString = @"%!@#$%^&*()?/>.<,:;\|}]{[_~+=-" + "\"";
            char[] specialCh = specialChString.ToCharArray();
            foreach (char ch in specialCh)
            {
                if (input.Contains(ch))
                    return false;
            }
            return true;
        }        
        
    }
}