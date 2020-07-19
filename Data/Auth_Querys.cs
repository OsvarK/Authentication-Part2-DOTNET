using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using AuthenticationAPI.Models;
using AuthenticationAPI.Security;

namespace CrudBackend.Data.Queries
{
    public class Auth_Querys
    {
        // Creates a new user and return UserID
        public int CreateNewUser(Auth_UserModel user)
        {
            string queryString = "INSERT INTO Users (Username, Email, FirstName, LastName, Password, GoogleSubjectID, IsAdmin) VALUES (@Username, @Email, @FirstName, @LastName, @Password, @GoogleSubjectID, @IsAdmin)";
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
            cmd.Parameters.Add(new MySqlParameter("@GoogleSubjectID", user.GoogleSubjectID));
            cmd.Parameters.Add(new MySqlParameter("@IsAdmin", user.IsAdmin));

            cmd.ExecuteScalar();
            connection.Close();

            return CheckIfPasswordIsCorrect(user.Username, user.Password);
        }

        // Returns users data Username, Email, FirstName, LastName
        public Auth_UserModel GetUsersData(string userID, string username)
        {
            string queryString = "SELECT * FROM Users WHERE UserID=@UserID AND Username=@Username";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", userID));
            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            Auth_UserModel user = new Auth_UserModel();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    user.UserID = int.Parse(RemoveSpacesInString(reader[0].ToString()));
                    user.Username = RemoveSpacesInString(reader[1].ToString());
                    user.Firstname = RemoveSpacesInString(reader[2].ToString());
                    user.Lastname = RemoveSpacesInString(reader[3].ToString());
                    user.Email = RemoveSpacesInString(reader[4].ToString());
                    user.Password = RemoveSpacesInString(reader[5].ToString());
                    user.GoogleSubjectID = RemoveSpacesInString(reader[6].ToString());
                    user.IsAdmin = Convert.ToBoolean(Convert.ToInt16(RemoveSpacesInString(reader[7].ToString())));
                }
            }
            connection.Close();
            return user;
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

        // Returns the user id if password is correct else return -1
        public int CheckIfPasswordIsCorrect(string username, string password)
        {
            string queryString = "SELECT UserID FROM Users WHERE Username=@Username AND Password=@Password";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));
            cmd.Parameters.Add(new MySqlParameter("@Password", password));

            int UserID = -1;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserID = reader.GetInt32(0);
                }
            }

            connection.Close();
            return UserID;
        }

        // Returns admin status
        public bool CheckIsUserisAdmin(string username)
        {
            string queryString = "SELECT IsAdmin FROM Users WHERE Username=@Username";
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
        public void EditUser(Auth_UserModel user) {
            string queryString = "UPDATE Users SET Username=@Username, Email=@Email, Firstname=@Firstname, Lastname=@Lastname, Password=@Password WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", user.UserID));
            cmd.Parameters.Add(new MySqlParameter("@Username", user.Username));
            cmd.Parameters.Add(new MySqlParameter("@Email", user.Email));
            cmd.Parameters.Add(new MySqlParameter("@Firstname", user.Firstname));
            cmd.Parameters.Add(new MySqlParameter("@Lastname", user.Lastname));
            cmd.Parameters.Add(new MySqlParameter("@Password", user.Password));

            cmd.ExecuteScalar();
            connection.Close();
        }

       //// Returns users data from google id.
       //public Auth_UserModel GetUserFromGoogleID(string googleID)
       //{
       //    string queryString = "SELECT * FROM Users WHERE GoogleSubjectID=@GoogleSubjectID";
       //    MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());
       //
       //    MySqlCommand cmd = new MySqlCommand();
       //    cmd.Connection = connection;
       //    cmd.CommandText = queryString;
       //    connection.Open();
       //    cmd.Parameters.Add(new MySqlParameter("@GoogleSubjectID", googleID));
       //
       //    Auth_UserModel user = new Auth_UserModel();
       //    using (MySqlDataReader reader = cmd.ExecuteReader())
       //    {
       //        if (reader.Read())
       //        {
       //            user.UserID = int.Parse(RemoveSpacesInString(reader[0].ToString()));
       //            user.Username = RemoveSpacesInString(reader[1].ToString());
       //            user.Firstname = RemoveSpacesInString(reader[2].ToString());
       //            user.Lastname = RemoveSpacesInString(reader[3].ToString());
       //            user.Email = RemoveSpacesInString(reader[4].ToString());
       //            user.Password = RemoveSpacesInString(reader[5].ToString());
       //            user.GoogleSubjectID = RemoveSpacesInString(reader[6].ToString());
       //            user.IsAdmin = Convert.ToBoolean(Convert.ToInt16(RemoveSpacesInString(reader[7].ToString()))); 
       //        }
       //    }
       //    connection.Close();
       //    return user;
       //}




        // SQL creates spaces to fill all the varachars, remove them.
        string RemoveSpacesInString(string str)
        {
            if (str == null)
                return null;
            return str.Replace(" ", String.Empty);
        }
    }
}
