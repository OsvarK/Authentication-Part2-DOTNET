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

        public bool CheckIfUserIsAdmin(string username)
        {
            return authQuery.CheckIsUserisAdmin(username);
        }

        public int CreateNewUser(Auth_RegisterModel newUser)
        {
            Auth_UserModel user = new Auth_UserModel();
            user.RegisterModelToSqlModel(newUser);
            // Database interaction
            return authQuery.CreateNewUser(user);
        }

        public Auth_UserModel GetUsersData(List<string> claims)
        {
            // Database interaction
            // Claims[0] = UserID, claims[1] = username
            return authQuery.GetUsersData(claims[0], claims[1]);
        }

        public bool DoesUserExist(string username, string email)
        {
            // Database interaction
            return authQuery.DoesUserExist(username, email);
        }

        public Auth_UserModel EditUser(Auth_EditProfileModel editUser, List<string> claims){

            Auth_UserModel beforeUpdateUserData = authQuery.GetUsersData(claims[0], claims[1]);
            Auth_UserModel user = new Auth_UserModel();
            user.EditModelToSqlModel(editUser);

            // this fields are not allwed to be changed here.   
            user.UserID = beforeUpdateUserData.UserID;
            user.GoogleSubjectID = beforeUpdateUserData.GoogleSubjectID;
            user.IsAdmin = beforeUpdateUserData.IsAdmin;

            // if they fields are empty, populate with old data.
            if (string.IsNullOrEmpty(user.Username))
                user.Username = beforeUpdateUserData.Username;
            if(string.IsNullOrEmpty(user.Firstname))
                user.Firstname = beforeUpdateUserData.Firstname;
            if(string.IsNullOrEmpty(user.Lastname))
                user.Lastname = beforeUpdateUserData.Lastname;
            if(string.IsNullOrEmpty(user.Email))
                user.Email = beforeUpdateUserData.Email;
            if (string.IsNullOrEmpty(user.Password))
                user.Password = beforeUpdateUserData.Password;
            else
                user.Password = Hash.HashPassword(user.Password);

            // Database interaction
            authQuery.EditUser(user);
            // Return the user
            return user;
        }

        // Check if password match the password in database.
        public int PasswordMatch(string username, string password)
        {
            return authQuery.CheckIfPasswordIsCorrect(username, password);
        }

    }
}
