using System;

namespace AuthenticationAPI.Models
{
    public class Auth_UserModel
    {
        // This model is same as the User table
        public int UserID { get; set; }
        public string Username { get; set; }  
        public string Firstname { get; set; } 
        public string Lastname { get; set; }  
        public string Email { get; set; }       
        public string Password { get; set; }
        public string GoogleSubjectID { get; set; }
        public bool IsAdmin { get; set; }

        public void RegisterModelToSqlModel(Auth_RegisterModel registerModel)
        {
            Username = registerModel.Username;
            Email = registerModel.Email;
            Firstname = registerModel.Firstname;
            Lastname = registerModel.Lastname;
            Password = registerModel.Password;
            GoogleSubjectID = ""; 
            IsAdmin = false;
        }

        public void EditModelToSqlModel(Auth_EditProfileModel editModel)
        {
            Username = editModel.Username;
            Email = editModel.Email;
            Firstname = editModel.Firstname;
            Lastname = editModel.Lastname;
            Password = editModel.Password;
            GoogleSubjectID = "";
            IsAdmin = false;
        }


        //CREATE TABLE `Users` (
        //    `UserID` INT(255) NOT NULL AUTO_INCREMENT,
        //    `Username` VARCHAR(255),
        //    `Firstname` VARCHAR(255),
        //    `Lastname` VARCHAR(255),
        //    `Email` VARCHAR(255),
        //    `Password` VARCHAR(255),
        //    `GoogleSubjectID` VARCHAR(255),
        //    `IsAdmin` VARCHAR(255),
        //    PRIMARY KEY(`UserID`)
        //);

    }

}
