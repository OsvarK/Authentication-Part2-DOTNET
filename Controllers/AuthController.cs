using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;

using AuthenticationAPI.Security;
using AuthenticationAPI.Models;
using AuthenticationAPI.Logic;
using Google.Apis.Auth;

namespace AuthenticationAPI.Controllers
{
    // HTTP SETTINGS
    [EnableCors("AuthCorsPolicy")]
    [Route("api/auth")]
    [ApiController]

    public class AuthController : Controller
    {
        // Instantiate 
        InputValidations inputValidations = new InputValidations();
        Auth_Actions authAction = new Auth_Actions();

        // Disable the ability to create an account.
        private bool _DisableSignup;

        public AuthController()
        {
            // check if we allow users to be able to signup. refers to appsetings.json 
            _DisableSignup = ConfigContex.UserRegisteringDsabled();
        }

        int Authenticate()
        {
            // Get Token from Cookies
            string key = "token";
            var value = Request.Cookies[key];
            // return the UserID if authorized else return -1
            return AuthToken.Authorization(value);
        }

        // return false if it fail
        bool UpdateToken(Auth_UserModel user)
        {
            try
            {
                Response.Cookies.Delete("token");
                string token = AuthToken.GenerateAuthToken(user);
                string key = "token";
                string value = token;
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Append(key, value, cookieOptions);
                return true;
            }
            catch
            {
                return false;
            }
        }


        // API CALLS --------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------

        // POST: api/auth/createuser ----------------------------------------------------------------------
        // Require user data, creates user.
        [HttpPost("signup")]
        public IActionResult CreateNewUser([FromBody] Auth_RegisterUserModel newUser)
        {
            Console.WriteLine("api/auth/signup");
            
            // Make it posible to disable this post request!
            if (_DisableSignup)
                return StatusCode(405, "Creating accounts have been disabled.");

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_RegisterUserModelValidation(newUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            newUser.password = Hash.HashPassword(newUser.password);

            // Check if user already exist
            if (authAction.DoesEmailExist(newUser.email))
                return StatusCode(405, "Email already exist.");
            if (authAction.DoesUsernameExist(newUser.username))
                return StatusCode(405, "Username already exist.");

            // Create new user, return user id, update token.
            int userID = authAction.CreateNewUserUsingRegisterModel(newUser);         
            if (UpdateToken(authAction.GetUsersData(userID)))
                return Ok("Successfully created your account!");
            else
                return StatusCode(500, "something went wrong");
        }

        // POST: api/auth/edituser
        // Require user data, changes user data
        [HttpPost("edituser")]
        public IActionResult EditUser([FromBody] Auth_EditUserModel editUser)
        {
            Console.WriteLine("api/auth/edituser");
            
            // Check for Authentication claims
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");

            // Check if user already exist
            if (authAction.DoesEmailExist(editUser.email))
                return StatusCode(405, "Email already exist.");
            if (authAction.DoesUsernameExist(editUser.username))
                return StatusCode(405, "Username already exist.");

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_EditUserModelValidation(editUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            // updates and returns the new user information.     
            Auth_UserModel user = authAction.EditUser(editUser, userID);
            if (UpdateToken(user))
                return Ok("Successfully edited your account!");
            else
                return StatusCode(500, "something went wrong");
        }

        // POST: api/auth/changepassword
        // changes user password
        [HttpPost("changepassword")]
        public IActionResult ChangePassword([FromBody] Auth_ChangeUserPasswordModel changeUserPassword)
        {
            Console.WriteLine("api/auth/changepassword");

            // Check for Authentication claims
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");

            // Check if account is a external linked account.
            Tuple<bool, string> isAccountExternal = authAction.IsAccountLinkedToAlternativeAuth(userID);
            if (isAccountExternal.Item1)
                return StatusCode(405, "Your account was created using " + isAccountExternal.Item2 + ", visit them to change your password");

            // Validate user inputed data
           Tuple<bool, string> validation = inputValidations.Auth_ChangePasswordModelValidation(changeUserPassword);
           if (!validation.Item1)
               return StatusCode(405, validation.Item2);

            // validate the validation password.
            if (authAction.PasswordMatch(authAction.GetUsersData(userID).username, changeUserPassword.currentPassword) == -1)
                return StatusCode(405, "Validation failed, password incorrect.");

            // return
            Tuple<bool, string> returnedStatus = authAction.ChangePassword(userID, changeUserPassword);
            if (returnedStatus.Item1)
                return Ok("Successfully changed password");
            else
                return StatusCode(500, returnedStatus.Item2);
        }

        // GET: api/auth/auth ----------------------------------------------------------------------
        // Require auth token, return user data.
        [HttpGet("auth")]
        public IActionResult Auth()
        {
            Console.WriteLine("api/auth/auth");

            // Check for Authentication claims.
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");
            Auth_ReturnUserModel returnUser = UserModelConverter.UserModel_To_ReturnUserModel(authAction.GetUsersData(userID));
            returnUser.DefineLinkedAccountStatus();
            // return users data.
            return Ok(returnUser);
        }

        // GET: api/auth/authgoogle ----------------------------------------------------------------------
        // Require auth token, return user data.
        [HttpPost("authgoogle")]
        public IActionResult authgoogle([FromBody] string googleTokenID)
        {
            Console.WriteLine("api/auth/authgoogle");
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(googleTokenID, new GoogleJsonWebSignature.ValidationSettings()).Result;
                Auth_UserModel user = new Auth_UserModel();
                user.email = payload.Email;
                user.firstname = payload.GivenName;
                user.lastname = payload.FamilyName;
                user.googleSubjectID = payload.Subject;
                user = authAction.LoginUsingGoogle(user);
                if (UpdateToken(user))
                    return Ok("Successfully logged in to your account!");
                else
                    return StatusCode(500, "something went wrong");
            }
            catch (Exception)
            {
                return StatusCode(405, "Something went wrong");
            }   
        }

        // POST: api/auth/logout ----------------------------------------------------------------------
        // Require auth token, terminate auth token.
        [HttpGet("logout")]
        public IActionResult logout()
        {
            Console.WriteLine("api/auth/logout");

            // Check for Authentication claims
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");

            Response.Cookies.Delete("token");
            return Ok("Authorization was terminated.");
        }

        // POST: api/auth/login ----------------------------------------------------------------------
        // Require username and password, return token.
        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth_LoginUserModel loginUser)
        {
            Console.WriteLine("api/auth/login");
          
            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_LoginUserModelValidation(loginUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            loginUser.password = Hash.HashPassword(loginUser.password);

            int userId = authAction.PasswordMatch(loginUser.emailOrUsername, loginUser.password);

            if (userId == -1)
                return StatusCode(405, "Incorrect password.");
            else
            {
                if (UpdateToken(authAction.GetUsersData(userId)))
                    return Ok("Successfully logged in to your account!");
                else
                    return StatusCode(500, "something went wrong");
            }
        }

        // GET: api/auth/isadmin ----------------------------------------------------------------------
        // Returns if user is admin
        [HttpGet("isadmin")]
        public IActionResult admin()
        {
            Console.WriteLine("api/auth/isadmin");
            // Check for Authentication claims
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");

            return Ok("You admin status is: " + authAction.CheckIfUserIsAdmin(userID));
        }

        // GET: api/auth/isaccountlinked ----------------------------------------------------------------------
        // Returns if user is admin
        [HttpGet("isaccountlinked")]
        public IActionResult IsAccountLinked()
        {
            Console.WriteLine("api/auth/isaccountlinked");
            // Check for Authentication claims
            int userID = Authenticate();
            if (userID == -1)
                return StatusCode(405, "Authorization token is not valid.");

            Tuple<bool, string> isAccountExternal = authAction.IsAccountLinkedToAlternativeAuth(userID);

            if (isAccountExternal.Item1)
                return Ok(isAccountExternal.Item2);
            else
                return Ok("None");
        }
    }
}