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

            cmd.Parameters.Add(new MySqlParameter("@Username", user.username));
            cmd.Parameters.Add(new MySqlParameter("@Email", user.email));
            cmd.Parameters.Add(new MySqlParameter("@FirstName", user.firstname));
            cmd.Parameters.Add(new MySqlParameter("@LastName", user.lastname));
            cmd.Parameters.Add(new MySqlParameter("@Password", user.password));
            cmd.Parameters.Add(new MySqlParameter("@GoogleSubjectID", user.googleSubjectID));
            cmd.Parameters.Add(new MySqlParameter("@IsAdmin", false));

            cmd.ExecuteScalar();
            connection.Close();

            return CheckIfPasswordIsCorrect(user.email, user.password); // <- returns userID
        }

        // Returns users data
        public Auth_UserModel GetUsersData(int userID)
        {
            string queryString = "SELECT * FROM Users WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", userID));

            Auth_UserModel user = new Auth_UserModel();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    user.userID = int.Parse(RemoveSpacesInString(reader[0].ToString()));
                    user.username = RemoveSpacesInString(reader[1].ToString());
                    user.firstname = RemoveSpacesInString(reader[2].ToString());
                    user.lastname = RemoveSpacesInString(reader[3].ToString());
                    user.email = RemoveSpacesInString(reader[4].ToString());
                    user.password = RemoveSpacesInString(reader[5].ToString());
                    user.googleSubjectID = RemoveSpacesInString(reader[6].ToString());
                    user.isAdmin = Convert.ToBoolean(Convert.ToInt16(RemoveSpacesInString(reader[7].ToString())));
                }
            }
            connection.Close();
            return user;
        }
        // Changes pasword of user
        public void ChangePassword(int userID, string password)
        {
            string queryString = "UPDATE Users SET Password=@Password WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Password", password));
            cmd.Parameters.Add(new MySqlParameter("@UserID", userID));

            cmd.ExecuteReader();

            connection.Close();
        }


        // Return true if email exist
        public bool DoesEmailExist(string email)
        {
            string queryString = "SELECT Email FROM Users WHERE Email=@Email";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Email", email));

            MySqlDataReader reader = cmd.ExecuteReader();

            bool returnParam = reader.HasRows;
            connection.Close();
            return returnParam;
        }

        // Return true if account is linked, add more values to add more linked options.
        public Tuple<bool, string> IsAccountLinkedToAlternativeAuth(int userID)
        {
            string queryString = "SELECT GoogleSubjectID FROM Users WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", userID));
            string value = null;
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    value = RemoveSpacesInString(reader[0].ToString());
                }
            }        
            connection.Close();

            if (value == null)
                return Tuple.Create(false, "None");
            else
                return Tuple.Create(true, value);
        }

        // Return true if username exist
        public bool DoesUsernameExist(string username)
        {
            string queryString = "SELECT Username FROM Users WHERE Username=@Username";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", username));

            MySqlDataReader reader = cmd.ExecuteReader();

            bool returnParam = reader.HasRows;
            connection.Close();
            return returnParam;
        }

        // Returns the user id if password is correct else return -1
        public int CheckIfPasswordIsCorrect(string emailOrUsername, string password)
        {
            string queryString = "SELECT UserID FROM Users WHERE Email=@Email OR Username=@Username AND Password=@Password";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@Username", emailOrUsername));
            cmd.Parameters.Add(new MySqlParameter("@Email", emailOrUsername));
            cmd.Parameters.Add(new MySqlParameter("@Password", password));

            int userID = -1;

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    userID = reader.GetInt32(0);
                }
            }

            connection.Close();
            return userID;
        }

        // Returns admin status
        public bool CheckIsUserisAdmin(int UserID)
        {
            string queryString = "SELECT IsAdmin FROM Users WHERE UserID=@UserID";
            MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());

            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connection;
            cmd.CommandText = queryString;

            connection.Open();

            cmd.Parameters.Add(new MySqlParameter("@UserID", UserID));

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

            cmd.Parameters.Add(new MySqlParameter("@UserID", user.userID));
            cmd.Parameters.Add(new MySqlParameter("@Username", user.username));
            cmd.Parameters.Add(new MySqlParameter("@Email", user.email));
            cmd.Parameters.Add(new MySqlParameter("@Firstname", user.firstname));
            cmd.Parameters.Add(new MySqlParameter("@Lastname", user.lastname));
            cmd.Parameters.Add(new MySqlParameter("@Password", user.password));

            cmd.ExecuteScalar();
            connection.Close();
        }

       // Returns users data from google id.
       public Auth_UserModel GetUserFromGoogleID(string googleID)
       {
           string queryString = "SELECT * FROM Users WHERE GoogleSubjectID=@GoogleSubjectID";
           MySqlConnection connection = new MySqlConnection(ConfigContex.GetConnectionString());
       
           MySqlCommand cmd = new MySqlCommand();
           cmd.Connection = connection;
           cmd.CommandText = queryString;
           connection.Open();
           cmd.Parameters.Add(new MySqlParameter("@GoogleSubjectID", googleID));
       
           Auth_UserModel user = new Auth_UserModel();
           using (MySqlDataReader reader = cmd.ExecuteReader())
           {
               if (reader.Read())
               {
                    user.userID = int.Parse(RemoveSpacesInString(reader[0].ToString()));
                    user.username = RemoveSpacesInString(reader[1].ToString());
                    user.firstname = RemoveSpacesInString(reader[2].ToString());
                    user.lastname = RemoveSpacesInString(reader[3].ToString());
                    user.email = RemoveSpacesInString(reader[4].ToString());
                    user.password = RemoveSpacesInString(reader[5].ToString());
                    user.googleSubjectID = RemoveSpacesInString(reader[6].ToString());
                    user.isAdmin = Convert.ToBoolean(Convert.ToInt16(RemoveSpacesInString(reader[7].ToString())));
                }
           }
           connection.Close();
           return user;
       }

        // SQL creates spaces to fill all the varachars, remove them.
        string RemoveSpacesInString(string str)
        {
            if (str == null)
                return null;
            return str.Replace(" ", String.Empty);
        }
    }
}
