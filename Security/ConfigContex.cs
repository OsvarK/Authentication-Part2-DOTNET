using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AuthenticationAPI.Security
{
    public static class ConfigContex
    {
        // This class handle interaction with the config (appsettings.json)
        public static readonly IConfiguration configuration;

        public static IConfiguration GetConfiguration()
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            return configBuilder.Build();
        }

        // connection strtings

        public static string GetConnectionString()
        {
            return GetConfiguration().GetValue<string>("ConnectionStrings:Database");
        }
        public static string GetDropboxApiKey()
        {
            return GetConfiguration().GetValue<string>("ConnectionStrings:DropBoxApiKey");
        }

        // Security

        public static string GetJwtKey()
        {
            return GetConfiguration().GetValue<string>("Security:JwtKey");
        }
        public static string GetJwtIssuer()
        {
            return GetConfiguration().GetValue<string>("Security:JwtIssuer");
        }
        public static string GetJwtAudience()
        {
            return GetConfiguration().GetValue<string>("Security:JwtAudience");
        }
        public static string GetSalt()
        {
            return GetConfiguration().GetValue<string>("Security:Salt");
        }

        // Settings

        public static bool UserRegisteringDsabled()
        {
            return GetConfiguration().GetValue<bool>("Settings:DisableUserSignup");
        }
        public static int GetProfileImagePixelSize()
        {
            return GetConfiguration().GetValue<int>("Settings:ProfileImagePixelSize");
        }
        public static int GetProfileImageMaxSizeInBytes()
        {
            return GetConfiguration().GetValue<int>("Settings:ProfileImageMaxSizeInBytes");
        }
    }
}
