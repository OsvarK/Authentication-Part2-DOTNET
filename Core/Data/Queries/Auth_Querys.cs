using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AuthenticationAPI.Data.Context;
using AuthenticationAPI.Models;

namespace CrudBackend.Data.Queries
{
    public class Auth_Querys
    {
        // Creates a new user
        public void CreateNewUser(Auth_UserModel user)
        {
            string queryString = "INSERT INTO Users (Username, Email, FirstName, LastName, Password, Admin) VALUES (@Username, @Email, @FirstName, @LastName, @Password, @Admin)";
            DataContext dataContext = new DataContext();
            MySqlConnection connection = new MySqlConnection(dataContext.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", user.Username));
            cmd.Parameters.Add(new MySqlParameter("@Email", user.Email));
            cmd.Parameters.Add(new MySqlParameter("@FirstName", user.Firstname));
            cmd.Parameters.Add(new MySqlParameter("@LastName", user.Lastname));
            cmd.Parameters.Add(new MySqlParameter("@Password", user.Password));
            cmd.Parameters.Add(new MySqlParameter("@Admin", "0"));

            cmd.ExecuteScalar();
            connection.Close();
        }

        // Returns users data Username, Email, FirstName, LastName
        public List<string> GetUsersData(string username, string password)
        {
            string queryString = "SELECT Username, Email, FirstName, LastName FROM Users WHERE Username=@Username AND Password=@Password";
            DataContext dataContext = new DataContext();
            MySqlConnection connection = new MySqlConnection(dataContext.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            cmd.Parameters.Add(new MySqlParameter("@Password", password));

            Auth_UserModel user = new Auth_UserModel();
            List<string> returnData = new List<string>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    returnData.Add(RemoveSpacesInString(reader[0].ToString()));
                    returnData.Add(RemoveSpacesInString(reader[1].ToString()));
                    returnData.Add(RemoveSpacesInString(reader[2].ToString()));
                    returnData.Add(RemoveSpacesInString(reader[3].ToString()));
                }
            }

            connection.Close();
            return returnData;
        }

        // Return true if user or email exist
        public bool DoesUserExist(string username, string email)
        {
            string queryString = "SELECT Username, Email FROM Users WHERE Username=@Username OR Email=@Email";
            DataContext dataContext = new DataContext();
            MySqlConnection connection = new MySqlConnection(dataContext.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            cmd.Parameters.Add(new MySqlParameter("@Email", email));

            MySqlDataReader reader = cmd.ExecuteReader();

            bool returnParam = reader.HasRows;
            connection.Close();
            return returnParam;
        }

        // Returns passoword
        public string GetPassword(string username)
        {
            string queryString = "SELECT Password FROM Users WHERE Username=@Username";
            DataContext dataContext = new DataContext();
            MySqlConnection connection = new MySqlConnection(dataContext.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            int result = cmd.ExecuteNonQuery();
            string password = null;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    password = reader[0].ToString();
                }
            }

            connection.Close();
            return RemoveSpacesInString(password);
        }

        // Returns admin status
        public bool CheckIsUserisAdmin(string username)
        {
            string queryString = "SELECT Admin FROM Users WHERE Username=@Username";
            DataContext dataContext = new DataContext();
            MySqlConnection connection = new MySqlConnection(dataContext.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            bool isAdmin = false;

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    isAdmin = reader.GetBoolean(0);
                }
            }

            connection.Close();
            return isAdmin;

        }


        // SQL creates spaces to fill all the varachars, remove them.
        string RemoveSpacesInString(string str)
        {
            if (str == null)
            {
                return null;
            }
            return str.Replace(" ", String.Empty);
        }
    }
}
