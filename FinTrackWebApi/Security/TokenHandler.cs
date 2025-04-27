// FinTrackWebApi.Security.TokenHandler.cs

using FinTrackWebApi.Models; // UserModel için eklendi
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic; // Claims listesi için eklendi
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims; // Claims için eklendi
using System.Security.Cryptography;
using System.Text;

namespace FinTrackWebApi.Security
{
    public static class TokenHandler
    {
        public static Token CreateToken(IConfiguration configuration, UserModel user) // UserModel parametresi eklendi
        {
            Token token = new Token();

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:SecurityKey"]));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Claims oluşturma
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Kullanıcı ID'si
                new Claim(ClaimTypes.Name, user.Username),                   // Kullanıcı Adı
                new Claim(ClaimTypes.Email, user.Email)                     // E-posta
                // Gerekirse başka claimler (örneğin roller) eklenebilir
            };

            token.Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Token:Expiration"]));

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: configuration["Token:Issuer"],
                audience: configuration["Token:Audience"],
                claims: claims, // Claims listesi eklendi
                expires: token.Expiration,
                signingCredentials: signingCredentials
            );

            byte[] numbers = new byte[32];
            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(numbers);
            }
            token.RefreshToken = Convert.ToBase64String(numbers);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            token.AccessToken = tokenHandler.WriteToken(jwtSecurityToken);
            return token;
        }
    }
}