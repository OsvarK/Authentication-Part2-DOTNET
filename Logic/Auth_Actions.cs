using System;
using System.Collections.Generic;
using AuthenticationAPI.Models;
using AuthenticationAPI.Security;
using CrudBackend.Data.Queries;


namespace AuthenticationAPI.Logic
{
    public class Auth_Actions
    {

        // Instantiate the database query class
        Auth_Querys authQuery = new Auth_Querys();

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
    }
}
