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

        List<string> Authenticate()
        {
            // Get Token from Cookies
            string key = "token";
            var value = Request.Cookies[key];
            // return list of claims | if claims is null, user is not auth.
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
        public IActionResult CreateNewUser([FromBody] Auth_RegisterModel newUser)
        {
            Console.WriteLine("api/auth/signup");
            
            // Make it posible to disable this post request!
            if (_DisableSignup)
                return StatusCode(405, "Creating accounts have been disabled.");

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_UserModelValidationSignup(newUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            newUser.Password = Hash.HashPassword(newUser.Password);

            // Check if user already exist
            if (authAction.DoesUserExist(newUser.Username, newUser.Email))
                return StatusCode(405, "Username or Email already exist.");

            // Create new user, return user id.
            int userId = authAction.CreateNewUser(newUser);         
            List<string> authentication = new List<string>() { 
                userId.ToString(), 
                newUser.Username 
            };
            Auth_UserModel user = authAction.GetUsersData(authentication);
            if (UpdateToken(user))
                return Ok("Successfully created your account!");
            else
                return StatusCode(500, "something went wrong");
        }

        // POST: api/auth/edituser
        // Require user data, changes user data
        [HttpPost("edituser")]
        public IActionResult EditUser([FromBody] Auth_EditProfileModel editUser)
        {
            Console.WriteLine("api/auth/edituser");
            
            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");
     
            // validate the validation password.
            if (editUser.ValidatedPassword == null || authAction.PasswordMatch(authentication[1], editUser.ValidatedPassword) == -1)
                return StatusCode(405, "Validation failed, password incorrect.");

            editUser.ValidatedPassword = Hash.HashPassword(editUser.ValidatedPassword);

            if (authAction.DoesUserExist(editUser.Username, editUser.Email))
                return StatusCode(405, "Username or Email already exist.");

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_UserModelValidationSignupEdit(editUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            // updates and returns the new user information.     
            Auth_UserModel user = authAction.EditUser(editUser, authentication);
            if (UpdateToken(user))
                return Ok("Successfully edited your account!");
            else
                return StatusCode(500, "something went wrong");
        }

        // GET: api/auth/auth ----------------------------------------------------------------------
        // Require auth token, return user data.
        [HttpGet("auth")]
        public IActionResult Auth()
        {
            Console.WriteLine("api/auth/auth");

            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            // Token is valid, returing there data.
            return Ok(authAction.GetUsersData(authentication));
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
                user.Email = payload.Email;
                user.Firstname = payload.GivenName;
                user.Lastname = payload.FamilyName;
                string userid = payload.Subject;
                return Ok(payload);
            }
            catch (Exception)
            {
                return StatusCode(405, "Something went wrong");
            }

            //// Check for Authentication claims
            //var authentication = Authenticate();
            //if (authentication == null)
            //    return StatusCode(405, "Authorization token is not valid.");
            //
            //if (!authAction.PasswordMatch(authentication[0], authentication[1]))
            //    return StatusCode(405, "Incorrect password.");
            //
            //// Token is valid, returing there data.
            
        }



        // POST: api/auth/logout ----------------------------------------------------------------------
        // Require auth token, terminate auth token.
        [HttpGet("logout")]
        public IActionResult logout()
        {
            Console.WriteLine("api/auth/logout");

            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            Response.Cookies.Delete("token");
            return Ok("Authorization was terminated.");
        }

        // POST: api/auth/login ----------------------------------------------------------------------
        // Require username and password, return token.
        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth_LoginModel loginUser)
        {
            Console.WriteLine("api/auth/login");
          
            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_UserModelValidationLogin(loginUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            loginUser.Password = Hash.HashPassword(loginUser.Password);

            int userId = authAction.PasswordMatch(loginUser.Username, loginUser.Password);

            if (userId == -1)
                return StatusCode(405, "Incorrect password.");
            else
            {
                Auth_UserModel user = new Auth_UserModel();
                List<string> authentication = new List<string>() { userId.ToString(), loginUser.Username };
                user = authAction.GetUsersData(authentication);
                if (UpdateToken(user))
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
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            return Ok("You admin status is: " + authAction.CheckIfUserIsAdmin(authentication[1]));
        }
    }
}