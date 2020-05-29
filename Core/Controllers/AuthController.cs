using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;

using AuthenticationAPI.Security;
using AuthenticationAPI.Models;
using Microsoft.AspNetCore.Http;
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
        [HttpPost("createuser")]
        public IActionResult CreateNewUser([FromBody] Auth_UserModel user)
        {
            

            // Make it posible to disable this post request!
            if (_DisableSignup)
                return StatusCode(405, "Creating accounts have been disabled.");

            // Validate user inputed data
            if (!inputValidations.Auth_UserModelValidationSignup(user))
                return StatusCode(405, "Data was not inputed correctly.");

            // Check if user already exist
            if (authAction.DoesUserExist(user.Username, user.Email))
                return StatusCode(405, "Username or Email already exist.");

            // Create new user, and return there auth token
            authAction.CreateNewUser(user);
            return Ok(user.getAuthToken());
        }

        // GET: api/auth/auth ----------------------------------------------------------------------
        // Require auth token, return user data.
        [HttpGet("auth")]
        public IActionResult Auth()
        {
            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            // Token is valid, returing there data.
            return Ok(authAction.GetUsersData(authentication));
        }



        // POST: api/auth/logout ----------------------------------------------------------------------
        // Require auth token, terminate auth token.
        [HttpGet("logout")]
        public IActionResult logout()
        {
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
            // Validate user inputed data
            if (!inputValidations.Auth_UserModelValidationLogin(user))
                return StatusCode(405, "Data was not inputed correctly.");

            if (!authAction.PasswordMatch(user))
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
            // Check for Authentication claims
            var authentication = Authenticate();
            if (authentication == null)
                return StatusCode(405, "Authorization token is not valid.");

            return Ok("You admin status is: " + authAction.CheckIfUserIsAdmin(authentication[0]));
        }
    }
}