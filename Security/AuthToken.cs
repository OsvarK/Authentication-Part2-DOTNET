using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AuthenticationAPI.Models;
using System.Security.Principal;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.DataProtection;

namespace AuthenticationAPI.Security
{
    public static class AuthToken
    {
        public static string GenerateAuthToken(Auth_UserModel user)
        {

            string jwtKey = ConfigContex.GetJwtKey();

            var SymmetricSecurityLey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            // Signing Credentials
            var signingCredentials = new SigningCredentials(SymmetricSecurityLey, SecurityAlgorithms.HmacSha256);

            // Claims
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.CHash, user.Password)
            };

            // Create Token
            var token = new JwtSecurityToken(
                issuer: ConfigContex.GetJwtIssuer(),
                audience: ConfigContex.GetJwtAudience(),
                claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: signingCredentials
                );
            // Encode Token
            var encodeToken = new JwtSecurityTokenHandler().WriteToken(token);
            // Return Token
            return encodeToken;
        }

        // Check if jwt is valid and if valid return list of claims (payload)
        public static List<string> ReadToken(string jwtInput)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = GetValidationParameters();

                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(jwtInput, validationParameters, out validatedToken);


                var hash = tokenHandler.CanReadToken(jwtInput);
                var token = tokenHandler.ReadJwtToken(jwtInput);
                

                // Re-serialize the Token Claims to just Type and Values
                var jwtPayload = JsonConvert.SerializeObject(token.Claims.Select(c => new { c.Type, c.Value }));
                List<string> claims = new List<string>();
                foreach (var c in token.Claims)
                {
                    claims.Add(c.Value);
                }
                // Validation was successful, return list of claims (payload)
                return claims;
            }
            catch
            {
                return null;
            }
        }

        // Returns validation parameter settings
        private static TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidIssuer = ConfigContex.GetJwtIssuer(),
                ValidAudience = ConfigContex.GetJwtIssuer(),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigContex.GetJwtKey()))
            };
        }

        // Authorization
        // returns claims. if return null user is not authorized.
        public static List<string> Authorization(string token)
        {
            // Check if Cookie exist and read token
            List<string> tokenClaims;
            try
            {
                tokenClaims = ReadToken(token);
            }
            catch
            {
                tokenClaims = null;
            }
            return tokenClaims;
        }
    }
}
