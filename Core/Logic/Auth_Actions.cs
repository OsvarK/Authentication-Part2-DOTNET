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
            //Create AuthToken and HashPassword
            user.Password = new Hash().HashPassword(user.Password);
           
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

        public bool PasswordMatch(Auth_UserModel user)
        {
            string password = new Hash().HashPassword(user.Password);

            if(authQuery.GetPassword(user.Username) == password)
            {
                // Password is correct
                return true;
            } else
            {
                // Password is not correct
                return false;
            }

        }

    }
}
