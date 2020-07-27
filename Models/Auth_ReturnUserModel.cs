namespace AuthenticationAPI.Models
{
    public class Auth_ReturnUserModel
    {

        // this model should always ONLY be used as return model to client.

        public int userID { get; set; }
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public string googleSubjectID { get; set; }
        public string linkedAccountProvider { get; set; }
        public bool linkedAccountStatus { get; set; }
        public bool isAdmin { get; set; }

        public void DefineLinkedAccountStatus()
        {
            // is account google linked
            if (!string.IsNullOrEmpty(googleSubjectID))
            {
                linkedAccountProvider = "Google";
                linkedAccountStatus = true;
                return;
            }
        }
    }
}
