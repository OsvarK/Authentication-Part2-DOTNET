using System;

namespace AuthenticationAPI.Models
{
    public class Auth_UserModel
    {
        // This model is same as the User table
        public int userID { get; set; }
        public string username { get; set; }
        public string firstname { get; set; } 
        public string lastname { get; set; }  
        public string email { get; set; }       
        public string password { get; set; }
        public string googleSubjectID { get; set; }
        public bool isAdmin { get; set; }


        //CREATE TABLE `Users` (
        //    `UserID` INT(255) NOT NULL AUTO_INCREMENT,
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
