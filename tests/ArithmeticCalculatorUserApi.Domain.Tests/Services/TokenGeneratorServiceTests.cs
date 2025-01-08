using ArithmeticCalculatorUserApi.Application.DTOs;
using ArithmeticCalculatorUserApi.Domain.Entities;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace ArithmeticCalculatorUserApi.Domain.Tests.Services
{
    public class TokenGeneratorServiceTests
    {
        private readonly TokenGeneratorService _tokenGeneratorService;
        private readonly string _jwtSecret = GenerateSecretKey();

        public TokenGeneratorServiceTests()
        {
            Environment.SetEnvironmentVariable("JWT_SECRET_KEY", _jwtSecret);
            _tokenGeneratorService = new TokenGeneratorService();
        }

        private static string GenerateSecretKey()
        {
            var keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            return Convert.ToBase64String(keyBytes);
        }

        [Fact]
        public void GenerateToken_ShouldGenerateValidJwtToken()
        {
            // Arrange
            var user = new UserDTO
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Status = "active"
            };

            // Act
            var token = _tokenGeneratorService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.Equal("testuser", jsonToken?.Claims.First(c => c.Type == "sub").Value);
            Assert.Equal(user.Id.ToString(), jsonToken?.Claims.First(c => c.Type == "id").Value);
            Assert.Equal("active", jsonToken?.Claims.First(c => c.Type == "status").Value);
            Assert.NotNull(jsonToken?.ValidTo);
        }

        [Fact]
        public void GenerateToken_ShouldUseCorrectSigningCredentials()
        {
            // Arrange
            var user = new UserDTO
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Status = "active"
            };

            // Act
            var token = _tokenGeneratorService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            Assert.Equal(SecurityAlgorithms.HmacSha256, jsonToken?.Header.Alg);
        }
    }
}
