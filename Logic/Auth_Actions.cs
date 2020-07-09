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

        public void CreateNewUser(Auth_UserModel user)
        {         
            // Database interaction
            authQuery.CreateNewUser(user);
        }

        public List<string> GetUsersData(List<string> claims)
        {
            // Database interaction
            // Claims[0] = Username, claims[1] = password
            return authQuery.GetUsersData(claims[0], claims[1]); 
        }

        public bool DoesUserExist(string username, string email)
        {
            // Database interaction
            return authQuery.DoesUserExist(username, email);
        }

        public Auth_UserModel EditUser(Auth_UserModel newAuth_UserModel, List<string> claims){
            List<string> beforeUpdateUserData = GetUsersData(claims);

            // Determine witch values that have been changed
            if(newAuth_UserModel.Username == null)
                newAuth_UserModel.Username = beforeUpdateUserData[1];
            if(newAuth_UserModel.Firstname == null)
                newAuth_UserModel.Firstname = beforeUpdateUserData[2];
            if(newAuth_UserModel.Lastname == null)
                newAuth_UserModel.Lastname = beforeUpdateUserData[3];
            if(newAuth_UserModel.Email == null)
                newAuth_UserModel.Email = beforeUpdateUserData[4];
            if (newAuth_UserModel.Password == null)
                newAuth_UserModel.Password = beforeUpdateUserData[5];
            else
                newAuth_UserModel.Password = Hash.HashPassword(newAuth_UserModel.Password);

            // beforeUpdateUserData[0] == UserID
            authQuery.EditUser(newAuth_UserModel, int.Parse(beforeUpdateUserData[0]));
            return newAuth_UserModel;
        }

        // Check if password match the password in database.
        public bool PasswordMatch(string username, string password)
        {
            if (authQuery.GetPassword(username) == password)
                return true;
            else
                return false;
        }

    }
}
