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
        public string ProfileImageUrl { get; set; }

        public void validate(string httpHost)
        {
            DefineLinkedAccountStatus();
            SetProfilePicture(httpHost);
        }

        private void DefineLinkedAccountStatus()
        {
            // is account google linked
            if (!string.IsNullOrEmpty(googleSubjectID))
            {
                linkedAccountProvider = "Google";
                linkedAccountStatus = true;
            }
        }

        private void SetProfilePicture(string httpHost)
        {
            if (string.IsNullOrEmpty(ProfileImageUrl) || string.IsNullOrWhiteSpace(ProfileImageUrl))
                ProfileImageUrl = "https://" + httpHost + "/images/blankProfile.png"; ;
        }
    }
}
