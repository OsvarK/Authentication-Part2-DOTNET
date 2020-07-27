using AuthenticationAPI.Models;
using System;
using System.Linq;

namespace AuthenticationAPI.Security
{
    public class InputValidations
    {
        // This class is for filtering and is supposed to be easy to modify
        public Tuple<bool, string> Auth_ChangePasswordModelValidation(Auth_ChangeUserPasswordModel changeUserPasswordModel)
        {
            string[] stringsToValidate =
            {
                changeUserPasswordModel.currentPassword,
                changeUserPasswordModel.newPassword,
            };
            Tuple<bool, string> doneGeneralValidation = GeneralInputChecker(stringsToValidate);
            Tuple<bool, string> donePasswordValidation = PasswordInputChecker(changeUserPasswordModel.newPassword);
            if (!donePasswordValidation.Item1)
                return donePasswordValidation;
            if (!doneGeneralValidation.Item1)
                return doneGeneralValidation;
            return Tuple.Create(true, String.Empty);
        }

        public Tuple<bool, string> Auth_EditUserModelValidation(Auth_EditUserModel editUser)
        {
            Tuple<bool, string> status = Tuple.Create(true, "Ok");
            if (isNotEmpty(editUser.firstname))
                if (AtleastOneSymbol(editUser.firstname) || editUser.firstname.Any(char.IsDigit))
                    return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if (isNotEmpty(editUser.lastname))
                if (!AtleastOneSymbol(editUser.lastname) || editUser.lastname.Any(char.IsDigit))
                    return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if(isNotEmpty(editUser.email))
                if (!IsValidEmail(editUser.email))
                    return Tuple.Create(false, "Email is not a valid email.");
            return status;
        }

        public Tuple<bool, string> Auth_RegisterUserModelValidation(Auth_RegisterUserModel newUser)
        {
            if(!IsValidEmail(newUser.email))
                return Tuple.Create(false, "Email is not a valid email.");

            string[] stringsToValidate = 
            {
                newUser.username,
                newUser.firstname,
                newUser.lastname,
                newUser.email,
                newUser.password
            };
            Tuple<bool, string> doneGeneralValidation = GeneralInputChecker(stringsToValidate);
            Tuple<bool, string> donePasswordValidation = PasswordInputChecker(newUser.password);
            Tuple<bool, string> doneUsernameValidation = UsernameInputChecker(newUser.username);
            if (AtleastOneSymbol(newUser.firstname) || AtleastOneSymbol(newUser.lastname) || newUser.firstname.Any(char.IsDigit) || newUser.lastname.Any(char.IsDigit))
                return Tuple.Create(false, "Your name should not contain any symbols or numbers");
            if (!doneUsernameValidation.Item1)
                return doneUsernameValidation;
            if (!doneGeneralValidation.Item1)
                return doneGeneralValidation;
            if (!donePasswordValidation.Item1)
                return donePasswordValidation;
            return Tuple.Create(true, String.Empty);
        }
        

        public Tuple<bool, string> Auth_LoginUserModelValidation(Auth_LoginUserModel loginUser)
        {
            string[] stringsToValidate =
            {
                loginUser.emailOrUsername,
                loginUser.password
            };

            // if the inputed data is not email
            if (!IsValidEmail(loginUser.emailOrUsername))
            {
                Tuple<bool, string> doneUsernameValidation = UsernameInputChecker(loginUser.emailOrUsername);
                if (!doneUsernameValidation.Item1)
                    return doneUsernameValidation;
            }

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
            if (!AtleastOneSymbol(input))
                return Tuple.Create(false, "Password needs atleast one special character.");
            return Tuple.Create(true, string.Empty);
        }

        Tuple<bool, string> UsernameInputChecker(string input)
        {
            if (input.Length < 5)
                return Tuple.Create(false, "Username needs to be more or equal to 5 characters.");
            if (NoneAllowedSymbolsInUsername(input))
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
                    return true;
            }
            return false;
        }

        bool NoneAllowedSymbolsInUsername(string input)
        {
            string specialChString = @"%!@#$%^&*()?/>.<,:;\|}]{[~+=" + "\"";
            char[] specialCh = specialChString.ToCharArray();
            foreach (char ch in specialCh)
            {
                if (input.Contains(ch))
                    return true;
            }
            return false;
        }

    }
}