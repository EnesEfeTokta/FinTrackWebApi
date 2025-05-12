using FinTrackWebApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FinTrackWebApi.Security
{
    public static class TokenHandler
    {
        public static Token CreateToken(IConfiguration configuration, UserModel user)
        {
            Token token = new Token();

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:SecurityKey"] ?? "null"));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()), // Kullanıcı ID'si
                new Claim(ClaimTypes.Name, user.Username),                   // Kullanıcı Adı
                new Claim(ClaimTypes.Email, user.Email)                     // E-posta
                // TODO: Gerekirse başka claimler (örneğin roller) eklenebilir
            };

            token.Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(configuration["Token:Expiration"]));

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                issuer: configuration["Token:Issuer"],
                audience: configuration["Token:Audience"],
                claims: claims,
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