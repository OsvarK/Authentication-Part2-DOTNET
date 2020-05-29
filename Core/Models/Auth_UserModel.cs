namespace AuthenticationAPI.Models
{
    public class Auth_UserModel
    {
        

        private string AuthToken { get; set; }  // Shuld not be in database

        // UserID is also going to be the primary key in the table
        public string Username { get; set; }    // Same in database
        public string Firstname { get; set; }   // Same in database
        public string Lastname { get; set; }    // Same in database
        public string Email { get; set; }       // Same in database
        public string Password { get; set; }    // Same in database


        public string getAuthToken()
        {
            return AuthToken;
        }

        public void setAuthToken(string NewAuthToken)
        {
            AuthToken = NewAuthToken;
        }
    }
}
