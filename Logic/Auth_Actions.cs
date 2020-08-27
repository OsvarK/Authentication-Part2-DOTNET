using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using AuthenticationAPI.Models;
using AuthenticationAPI.Security;
using CrudBackend.Data.Queries;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace AuthenticationAPI.Logic
{
    public class Auth_Actions
    {
        // Instantiate the database query class
        Auth_Querys authQuery = new Auth_Querys();


        // -------------------------------------------------------------------------------------
        // Functions 

        public bool CheckIfUserIsAdmin(int userID)
        {
            return authQuery.CheckIsUserisAdmin(userID);
        }

        public int CreateNewUserUsingRegisterModel(Auth_RegisterUserModel newUser)
        {
            Auth_UserModel user = UserModelConverter.RegisterUserModel_To_UserModel(newUser);
            return authQuery.CreateNewUser(user);
        }

        public Auth_UserModel GetUsersData(int userID)
        {
            return authQuery.GetUsersData(userID);
        }

        public bool DoesEmailExist(string email)
        {
            return authQuery.DoesEmailExist(email);
        }

        public bool DoesUsernameExist(string username)
        {
            return authQuery.DoesUsernameExist(username);
        }

        public Auth_UserModel EditUser(Auth_EditUserModel editUser, int userID){

            Auth_UserModel beforeUpdateUserData = authQuery.GetUsersData(userID);
            Auth_UserModel user = UserModelConverter.EditUserModel_To_UserModel(editUser);

            // this fields are not allwed to be changed here.   
            user.userID = beforeUpdateUserData.userID;
            user.googleSubjectID = beforeUpdateUserData.googleSubjectID;
            user.isAdmin = beforeUpdateUserData.isAdmin;

            // if they fields are empty, populate with old data.
            if (string.IsNullOrEmpty(user.username))
                user.username = beforeUpdateUserData.username;
            if (string.IsNullOrEmpty(user.firstname))
                user.firstname = beforeUpdateUserData.firstname;
            if(string.IsNullOrEmpty(user.lastname))
                user.lastname = beforeUpdateUserData.lastname;
            if (string.IsNullOrEmpty(user.email))
                user.email = beforeUpdateUserData.email;
            if (string.IsNullOrEmpty(user.password))
                user.password = beforeUpdateUserData.password;
            else
                user.password = Hash.HashPassword(user.password);

            // Database interaction
            authQuery.EditUser(user);
            // Return the user
            return user;
        }

        // Check if password match the password in database.
        public int PasswordMatch(string emailOrUsername, string password)
        {
            return authQuery.CheckIfPasswordIsCorrect(emailOrUsername, password);
        }

        public Auth_UserModel LoginUsingGoogle(Auth_UserModel user)
        {
            // Check if user already have an account that is linked to google.
            Auth_UserModel fetchedUser = authQuery.GetUserFromGoogleID(user.googleSubjectID);

            if (fetchedUser.googleSubjectID == null)
            {
                // No account found, create account.
                user.username = user.googleSubjectID + "@Google"; //<- a normal user cannot create a account using @ symbol. so we dont need to check if username already exist.
                authQuery.CreateNewUser(user); 
                user = authQuery.GetUserFromGoogleID(user.googleSubjectID);

            } else
            {
                // Account found
                user.userID = fetchedUser.userID;
                user.username = fetchedUser.username;
                user = fetchedUser;
            }
            return user;
        }

        public Tuple<bool, string> ChangePassword(int userID, Auth_ChangeUserPasswordModel changeUserPasswordModel)
        {
            if (!IsAccountLinkedToAlternativeAuth(userID).Item1)
            {
                try
                {
                    authQuery.ChangePassword(userID, changeUserPasswordModel.newPassword);
                    return Tuple.Create(true, String.Empty);
                }
                catch
                {
                    return Tuple.Create(false, "Error sending password to database");
                }
            }
            return Tuple.Create(false, "Cannot change password on a linked acount.");
        }

        public Tuple<bool, string> IsAccountLinkedToAlternativeAuth(int userID)
        {
            return authQuery.IsAccountLinkedToAlternativeAuth(userID);
        }

        public void DeleteAccount(int userID)
        {
            authQuery.DeleteAccount(userID);
        }

        public async Task<Tuple<bool, string>> UploadUserProfilePictureAsync(IWebHostEnvironment env, IFormFile imagefile, int userId)
        {
            if (imagefile == null || imagefile.Length < 0 || imagefile.Length > ConfigContex.GetProfileImageMaxSizeInBytes())
                return Tuple.Create(false, "File is to large or none existing");


            // Start image upload
            string imagePath = @"\Images\";
            string uploadPath = env.WebRootPath + imagePath;

            // Create Directory
            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Create file uniq name
            var fileName = ImageProcessing.GenerateUniqFileNameFromOldName(imagefile.FileName);
            imagePath = imagePath + @"\";
            var filePath = @".." + Path.Combine(imagePath, fileName);
            string fullPath = uploadPath + fileName;

            
            try
            {
                // Upload file to local storage (wwwroot)
                using (var img = Image.FromStream(imagefile.OpenReadStream()))
                {
                    // Copy image, resize it, make new image                      
                    int newSize = ConfigContex.GetProfileImagePixelSize(); // Get size from appsettings
                    Bitmap resultImage = ImageProcessing.Resize(img, newSize, newSize);
                    resultImage.Save(uploadPath + fileName);
                }

                // Upload file to dropbox, then get shared link
                string sharedUrl = await DropboxApi.Upload(fullPath, fileName, true);
                // Upload url to database
                //authQuery.UploadProfileImageUrlToDB(sharedUrl, userId);
                // Delete local stored image
                File.Delete(fullPath);
                // Succes
                return Tuple.Create(true, "Profile image successfully created.");
            }
            catch
            {
                return Tuple.Create(false, "Error in uploading file, check if its the correct extenstion (PNG, JPG).");
            }          
        }
    }
}
