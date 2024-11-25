using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ArithmeticCalculatorUserApi.Infrastructure.Security
{
    public class JwtTokenValidator
    {
        private readonly string _secretKey;

        public JwtTokenValidator(string secretKey)
        {
            _secretKey = secretKey;
        }

        public bool ValidateToken(string token, out int userId)
        {
            userId = 0;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Convert.FromBase64String(_secretKey);

            try
            {
                var claims = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    userId = int.Parse(claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
