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

namespace AuthenticationAPI.Security
{
    public class AuthToken
    {

        private readonly IConfiguration _configuration;
        private string _secretKey;

        public AuthToken()
        {
            _configuration = new ConfigContex().configuration;
            _secretKey = _configuration["Security:JwtKey"];
        }

        public string GenerateAuthToken(Auth_UserModel user)
        {
            var SymmetricSecurityLey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
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
                issuer: _configuration["Security:JwtIssuer"],
                audience: _configuration["Security:JwtAudience"],
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
        public List<string> ReadToken(string jwtInput)
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
        private TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateLifetime = true,
                ValidIssuer = _configuration["Security:JwtIssuer"],
                ValidAudience = _configuration["Security:JwtAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey))
            };
        }

        // Authorization
        // returns claims. if return null user is not authorized.
        public List<string> Authorization(string token)
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
