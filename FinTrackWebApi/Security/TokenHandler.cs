using FinTrackWebApi.Dtos;
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
        public static Token CreateToken(IConfiguration configuration, int id, string name, string email, IEnumerable<string> roles)
        {
            Token token = new Token();

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Token:SecurityKey"] ?? "null"));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()), // Kullanıcı ID'si
                new Claim(ClaimTypes.Name, name),                   // Kullanıcı Adı
                new Claim(ClaimTypes.Email, email)                     // E-posta
            };

            if (roles != null && roles.Any())
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

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