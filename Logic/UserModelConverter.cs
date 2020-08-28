using AuthenticationAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Logic
{
    public static class UserModelConverter
    {
        public static Auth_RegisterUserModel EditUserModel_To_RegisterUserModel(Auth_EditUserModel auth_EditProfileModel)
        {
            Auth_RegisterUserModel auth_RegisterModel = new Auth_RegisterUserModel();

            auth_RegisterModel.username = auth_EditProfileModel.username;
            auth_RegisterModel.email = auth_EditProfileModel.email;
            auth_RegisterModel.firstname = auth_EditProfileModel.firstname;
            auth_RegisterModel.lastname = auth_EditProfileModel.lastname;

            return auth_RegisterModel;
        }

        public static Auth_ReturnUserModel UserModel_To_ReturnUserModel(Auth_UserModel auth_UserModel)
        {
            Auth_ReturnUserModel auth_ReturnUserModel = new Auth_ReturnUserModel();

            auth_ReturnUserModel.userID = auth_UserModel.userID;
            auth_ReturnUserModel.username = auth_UserModel.username;
            auth_ReturnUserModel.firstname = auth_UserModel.firstname;
            auth_ReturnUserModel.lastname = auth_UserModel.lastname;
            auth_ReturnUserModel.email = auth_UserModel.email;
            auth_ReturnUserModel.googleSubjectID = auth_UserModel.googleSubjectID;
            auth_ReturnUserModel.isAdmin = auth_UserModel.isAdmin;
            auth_ReturnUserModel.profileImageUrl = auth_UserModel.profileImageUrl;
            return auth_ReturnUserModel;
        }

        public static Auth_UserModel RegisterUserModel_To_UserModel(Auth_RegisterUserModel registerModel)
        {
            Auth_UserModel auth_UserModel = new Auth_UserModel();

            auth_UserModel.username = registerModel.username;
            auth_UserModel.email = registerModel.email;
            auth_UserModel.firstname = registerModel.firstname;
            auth_UserModel.lastname = registerModel.lastname;
            auth_UserModel.password = registerModel.password;
            auth_UserModel.googleSubjectID = "";
            auth_UserModel.isAdmin = false;

            return auth_UserModel;
        }

        public static Auth_UserModel EditUserModel_To_UserModel(Auth_EditUserModel editModel)
        {
            Auth_UserModel auth_UserModel = new Auth_UserModel();

            auth_UserModel.username = editModel.username;
            auth_UserModel.email = editModel.email;
            auth_UserModel.firstname = editModel.firstname;
            auth_UserModel.lastname = editModel.lastname;
            auth_UserModel.googleSubjectID = "";
            auth_UserModel.isAdmin = false;

            return auth_UserModel;
        }
    }
}
