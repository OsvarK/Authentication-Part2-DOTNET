using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AuthenticationAPI.Models;
using AuthenticationAPI.Security;

namespace CrudBackend.Data.Queries
{
    public class Auth_Querys
    {
        // Creates a new user
        public void CreateNewUser(Auth_UserModel user)
        {
            string queryString = "INSERT INTO Users (Username, Email, FirstName, LastName, Password, Admin) VALUES (@Username, @Email, @FirstName, @LastName, @Password, @Admin)";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

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
            string queryString = "SELECT * FROM Users WHERE Username=@Username AND Password=@Password";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            cmd.Parameters.Add(new MySqlParameter("@Password", password));

            List<string> returnData = new List<string>();

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {

                int hej = reader.FieldCount;
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        returnData.Add(RemoveSpacesInString(reader[i].ToString()));
                    }
                }
            }

            connection.Close();
            return returnData;
        }

        // Return true if user or email exist
        public bool DoesUserExist(string username, string email)
        {
            string queryString = "SELECT Username, Email FROM Users WHERE Username=@Username OR Email=@Email";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

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
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

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
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

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

        // Update User information
        public void EditUser(Auth_UserModel user, int userID) {
            string queryString = "UPDATE Users SET Username=@Username, Email=@Email, Firstname=@Firstname, Lastname=@Lastname, Password=@Password WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", userID));
            cmd.Parameters.Add(new MySqlParameter("@Username", user.Username));
            cmd.Parameters.Add(new MySqlParameter("@Email", user.Email));
            cmd.Parameters.Add(new MySqlParameter("@Firstname", user.Firstname));
            cmd.Parameters.Add(new MySqlParameter("@Lastname", user.Lastname));
            cmd.Parameters.Add(new MySqlParameter("@Password", user.Password));

            cmd.ExecuteScalar();
            connection.Close();
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
