using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;

using AuthenticationAPI.Security;
using AuthenticationAPI.Models;
using AuthenticationAPI.Logic;

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
        AuthToken authToken = new AuthToken();
        Hash hash = new Hash();

        // Disable the ability to create an account.
        private bool _DisableSignup = false;

        List<string> Authenticate()
        {
            // Get Token from Cookies
            string key = "token";
            var value = Request.Cookies[key];

            // return list of claims | if claims is null, user is not auth.
            return authToken.Authorization(value);
        }

        // POST: api/auth/createuser ----------------------------------------------------------------------
        // Require user data, creates user.
        [HttpPost("signup")]
        public IActionResult CreateNewUser([FromBody] Auth_UserModel user)
        {
            Console.WriteLine("api/auth/signup");
            user.Password = hash.HashPassword(user.Password);

            // Make it posible to disable this post request!
            if (_DisableSignup)
                return StatusCode(405, "Creating accounts have been disabled.");

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_UserModelValidationSignup(user);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            // Check if user already exist
            if (authAction.DoesUserExist(user.Username, user.Email))
                return StatusCode(405, "Username or Email already exist.");

            // Create new user, and return there auth token
            authAction.CreateNewUser(user);

            return Ok("Successfully created your account!");
        }

        // POST: api/auth/edituser
        // Require user data, changes user data
        [HttpPost("edituser")]
        public IActionResult EditUser([FromBody] Auth_UserModel user)
        {
            Console.WriteLine("api/auth/edituser");
            user.ValidatedPassword = hash.HashPassword(user.ValidatedPassword);

            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            if(user.ValidatedPassword == null || !authAction.PasswordMatch(authentication[0], user.ValidatedPassword)) // authentication[0] username
                return StatusCode(405, "Validation failed, password incorrect.");

            if (authAction.DoesUserExist(user.Username, user.Email))
                return StatusCode(405, "Username or Email already exist.");

            // updates and returns the new user information.
            user = authAction.EditUser(user, authentication);

            // Reset cookie
            Response.Cookies.Delete("token");
            string token = authToken.GenerateAuthToken(user);
            string key = "token";
            string value = token;
            CookieOptions cookieOptions = new CookieOptions();
            cookieOptions.Expires = DateTime.Now.AddDays(7);
            Response.Cookies.Append(key, value, cookieOptions);

            return Ok();           
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

            if (!authAction.PasswordMatch(authentication[0], authentication[1]))
                return StatusCode(405, "Incorrect password.");

            // Token is valid, returing there data.
            return Ok(authAction.GetUsersData(authentication));
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
        public IActionResult Login([FromBody] Auth_UserModel user)
        {
            Console.WriteLine("api/auth/login");
            user.Password = hash.HashPassword(user.Password);

            // Validate user inputed data
            Tuple<bool, string> validation = inputValidations.Auth_UserModelValidationLogin(user);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            if (!authAction.PasswordMatch(user.Username, user.Password))
                return StatusCode(405, "Incorrect password.");
            else
            {
                // Create token
                string token = authToken.GenerateAuthToken(user);
                // Set Cookie
                string key = "token";
                string value = token;
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(7);
                Response.Cookies.Append(key, value, cookieOptions);
                return Ok("Successfully authenticated.");
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

            return Ok("You admin status is: " + authAction.CheckIfUserIsAdmin(authentication[0]));
        }
    }
}