using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArithmeticCalculatorUserApi.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace ArithmeticCalculatorUserApi.Infrastructure.Security
{
    public class JwtTokenGenerator
    {
        private readonly string _jwtSecret;

        public JwtTokenGenerator(string jwtSecret)
        {
            _jwtSecret = jwtSecret;
        }

        public string GenerateToken(int userId, string username, string status)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim("id", userId.ToString()),
                new Claim("status", status),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "arithmetic-calculator",
                audience: "arithmetic-calculator",
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds((int)TokenConfiguration.ExpirationTimeInSeconds),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
