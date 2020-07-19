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


        public Tuple<bool, string> Auth_UserModelValidationSignupEdit(Auth_EditProfileModel editUser)
        {
            Tuple<bool, string> status = Tuple.Create(true, "Ok");
            if (isNotEmpty(editUser.Firstname))
                if (!AtleastOneSymbol(editUser.Firstname) || editUser.Firstname.Any(char.IsDigit))
                    return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if (isNotEmpty(editUser.Lastname))
                if (!AtleastOneSymbol(editUser.Lastname) || editUser.Lastname.Any(char.IsDigit))
                    return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if (isNotEmpty(editUser.Username))
                status = UsernameInputChecker(editUser.Username);
            if(isNotEmpty(editUser.Email))
                if (!IsValidEmail(editUser.Email))
                    return Tuple.Create(false, "Email is not a valid email.");
            if(isNotEmpty(editUser.Password))
                status = PasswordInputChecker(editUser.Password);
            return status;
        }

        public Tuple<bool, string> Auth_UserModelValidationSignup(Auth_RegisterModel newUser)
        {
            if(!IsValidEmail(newUser.Email))
                return Tuple.Create(false, "Email is not a valid email.");

            string[] stringsToValidate = 
            {
                newUser.Firstname,
                newUser.Lastname,
                newUser.Username,
                newUser.Email,
                newUser.Password
            };
            Tuple<bool, string> doneGeneralValidation = GeneralInputChecker(stringsToValidate);
            Tuple<bool, string> donePasswordValidation = PasswordInputChecker(newUser.Password);
            Tuple<bool, string> doneUsernameValidation = UsernameInputChecker(newUser.Username);

            if(!AtleastOneSymbol(newUser.Firstname) || !AtleastOneSymbol(newUser.Lastname) || newUser.Firstname.Any(char.IsDigit) || newUser.Lastname.Any(char.IsDigit))
                return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if (!doneGeneralValidation.Item1)
                return doneGeneralValidation;
            if (!donePasswordValidation.Item1)
                return donePasswordValidation;
            if (!doneUsernameValidation.Item1)
                return doneUsernameValidation;
            return Tuple.Create(true, String.Empty);
        }
        

        public Tuple<bool, string> Auth_UserModelValidationLogin(Auth_LoginModel loginUser)
        {
            string[] stringsToValidate =
            {
                loginUser.Username,
                loginUser.Password
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
                if (!isNotEmpty(data))
                    return Tuple.Create(false, "White spaces or empty fields are not allowed.");
            }
            return Tuple.Create(true, string.Empty);
        }

        bool isNotEmpty(string s)
        {
            if (s == null)
                return false;
            if (string.IsNullOrWhiteSpace(s))
                return false;
            return true;
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