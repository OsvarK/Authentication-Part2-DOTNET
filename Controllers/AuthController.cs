using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

using AuthenticationAPI.Security;
using AuthenticationAPI.Models;
using AuthenticationAPI.Logic;

using Google.Apis.Auth;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;

namespace AuthenticationAPI.Controllers
{
    // HTTP SETTINGS
    [EnableCors("AuthCorsPolicy")]
    [Route("api/auth")]
    [ApiController]

    public class AuthController : Controller
    {
        // Instantiate 
        private IWebHostEnvironment _env;
        private InputValidations _inputValidations = new InputValidations();
        private Auth_Actions _authAction = new Auth_Actions();
        
        // Global Variables
        private bool _DisableSignup; 
        
        // On class creation
        public AuthController(IWebHostEnvironment environment)
        {
            _env = environment;
            _DisableSignup = ConfigContex.UserRegisteringDsabled(); // <- check if we allow users to be able to signup. refers to appsetings.json 
        }

        // Functions --------------------------------------------------------------------------------------
        // ------------------------------------------------------------------------------------------------

        int Authenticate()
        {
            // Get Token from Cookies
            string key = "token";
            string value = Request.Cookies[key];
            // return the userID if authorized else return -1         
            return AuthToken.Authorization(value);
        }

        // return false if it fail
        bool UpdateToken(int userId)
        {
            try
            {
                Response.Cookies.Delete("token");
                string token = AuthToken.GenerateAuthToken(userId);
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
        // Requires: None-Authentication
        // Description: Registers a new user, then Authenticates it.
        [HttpPost("signup")]
        public IActionResult CreateNewUser([FromBody] Auth_RegisterUserModel newUser)
        {
            Console.WriteLine("api/auth/signup");
            
            // Make it posible to disable this post request!
            if (_DisableSignup)
                return StatusCode(405, "Creating accounts have been disabled.");

            // Validate user inputed data
            Tuple<bool, string> validation = _inputValidations.Auth_RegisterUserModelValidation(newUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            newUser.password = Hash.HashPassword(newUser.password);

            // Check if user already exist
            if (_authAction.DoesEmailExist(newUser.email))
                return StatusCode(405, "Email already exist.");
            if (_authAction.DoesUsernameExist(newUser.username))
                return StatusCode(405, "Username already exist.");

            // Create new user, return user id, update token.
            int userID = _authAction.CreateNewUserUsingRegisterModel(newUser); // <- to determien path to blank profile picture. (proberly is a better way, but for now im going with this)
            if (UpdateToken(userID))
                return Ok("Successfully created your account!");
            else
                return StatusCode(500, "something went wrong");
        }

        // POST: api/auth/edituser ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Edit the information of the authenticated user.
        [HttpPost("edituser")]
        public IActionResult EditUser([FromBody] Auth_EditUserModel editUser)
        {
            Console.WriteLine("api/auth/edituser");

            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            // Check if user already exist
            if (_authAction.DoesEmailExist(editUser.email))
                return StatusCode(405, "Email already exist.");
            if (_authAction.DoesUsernameExist(editUser.username))
                return StatusCode(405, "Username already exist.");

            // Validate user inputed data
            Tuple<bool, string> validation = _inputValidations.Auth_EditUserModelValidation(editUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);
            else if (UpdateToken(userID))
                return Ok("Successfully edited your account!");
            else
                return StatusCode(500, "something went wrong");
        }
         
        // POST: api/auth/changepassword ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Change the password of the authenticated user.
        [HttpPost("changepassword")]
        public IActionResult ChangePassword([FromBody] Auth_ChangeUserPasswordModel changeUserPassword)
        {
            Console.WriteLine("api/auth/changepassword");

            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            // Check if account is a external linked account.
            Tuple<bool, string> isAccountExternal = _authAction.IsAccountLinkedToAlternativeAuth(userID);
            if (isAccountExternal.Item1)
                return StatusCode(405, "Your account was created using " + isAccountExternal.Item2 + ", visit them to change your password");

            // Validate user inputed data
           Tuple<bool, string> validation = _inputValidations.Auth_ChangePasswordModelValidation(changeUserPassword);
           if (!validation.Item1)
               return StatusCode(405, validation.Item2);

            // validate the validation password.
            changeUserPassword.currentPassword = Hash.HashPassword(changeUserPassword.currentPassword);
            changeUserPassword.newPassword = Hash.HashPassword(changeUserPassword.newPassword);
            if (_authAction.PasswordMatch(_authAction.GetUsersData(userID).username, changeUserPassword.currentPassword) == -1)
                return StatusCode(405, "Validation failed, password incorrect.");

            // return
            Tuple<bool, string> returnedStatus = _authAction.ChangePassword(userID, changeUserPassword);
            if (returnedStatus.Item1)
                return Ok("Successfully changed password");
            else
                return StatusCode(500, returnedStatus.Item2);
        }

        // GET: api/auth/auth ----------------------------------------------------------------------
        // Requires: None-Authentication
        // Description: Determien if user is authenticated.
        [HttpGet("auth")]
        public IActionResult Auth()
        {
            Console.WriteLine("api/auth/auth");

            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            Auth_ReturnUserModel returnUser = UserModelConverter.UserModel_To_ReturnUserModel(_authAction.GetUsersData(userID));
            returnUser.validate(Request.Host.ToString());
            // return users data.
            return Ok(returnUser);
        }

        // POST: api/auth/authgoogle ----------------------------------------------------------------------
        // Requires: None-Authentication
        // Description: Login/Regiser an account from external source (Google).
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
                user = _authAction.LoginUsingGoogle(user);
            
                if (UpdateToken(user.userID))
                    return Ok("Successfully logged in to your account!");
                else
                    return StatusCode(500, "something went wrong");
            }
            catch (Exception)
            {
                return StatusCode(405, "Something went wrong");
            }  
            return Ok(googleTokenID);
        }

        // GET: api/auth/logout ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Terminate the authentication cookie (Logout).
        [HttpGet("logout")]
        public IActionResult logout()
        {
            Console.WriteLine("api/auth/logout");

            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            Response.Cookies.Delete("token");
            return Ok("Authorization was terminated.");
        }

        // POST: api/auth/login ----------------------------------------------------------------------
        // Requires: None-Authentication
        // Description: Login an user accoirding to the inputs, then authneticate the user.
        [HttpPost("login")]
        public IActionResult Login([FromBody] Auth_LoginUserModel loginUser)
        {
            Console.WriteLine("api/auth/login");
          
            // Validate user inputed data
            Tuple<bool, string> validation = _inputValidations.Auth_LoginUserModelValidation(loginUser);
            if (!validation.Item1)
                return StatusCode(405, validation.Item2);

            loginUser.password = Hash.HashPassword(loginUser.password);

            int userID = _authAction.PasswordMatch(loginUser.emailOrUsername, loginUser.password);

            if (userID == -1)
                return StatusCode(405, "Incorrect password.");
            else
            {
                if (UpdateToken(_authAction.GetUsersData(userID).userID))
                    return Ok("Successfully logged in to your account!");
                else
                    return StatusCode(500, "something went wrong");
            }
        }

        // GET: api/auth/isadmin ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Retrives information admin status from the authenticated account
        [HttpGet("isadmin")]
        public IActionResult admin()
        {
            Console.WriteLine("api/auth/isadmin");
            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            return Ok("You admin status is: " + _authAction.CheckIfUserIsAdmin(userID));
        }

        // GET: api/auth/isaccountlinked ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Retrives information linked status from the authenticated account
        [HttpGet("isaccountlinked")]
        public IActionResult IsAccountLinked()
        {
            Console.WriteLine("api/auth/isaccountlinked");
            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            Tuple<bool, string> isAccountExternal = _authAction.IsAccountLinkedToAlternativeAuth(userID);

            if (isAccountExternal.Item1)
                return Ok(isAccountExternal.Item2);
            else
                return Ok("None");
        }

        // GET: api/auth/deleteaccount ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Deletes the authenticated users account.
        [HttpGet("deleteaccount")]
        public IActionResult DeleteAccount()
        {
            Console.WriteLine("api/auth/deleteaccount");

            // Authenticate
            int userID = Authenticate();
            if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            Response.Cookies.Delete("token");
            _authAction.DeleteAccount(userID);
            return Ok("Authorization was terminated.");
        }


        // POST: api/auth/upload/profileimage ----------------------------------------------------------------------
        // Requires: Authentication
        // Description: Upload the authenticated users profile picture.
        [HttpPost("upload/profileimage")]
        public async Task<IActionResult> UploadAvatar(IFormFile imagefile)
        {
            Console.WriteLine("api/auth/upload/profileimage");
            // Authenticate
            // int userID = Authenticate();
            // if (userID == -1) { return StatusCode(405, "Authorization token is not valid."); }

            var result = await _authAction.UploadUserProfilePictureAsync(_env, imagefile, 0);
            if (result.Item1 == true)
                return Ok(result.Item2);
            else
                return StatusCode(404, result.Item2);
        }
    }
}