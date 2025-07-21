using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Domain.Tests.DTOs
{
    public class RefreshTokenDTOTests
    {
        [Fact]
        public void RefreshTokenDTO_ShouldInitializeWithDefaultValues()
        {
            // Act
            var refreshTokenDto = new RefreshTokenDTO();

            // Assert
            Assert.Equal(Guid.Empty, refreshTokenDto.UserId);
            Assert.Null(refreshTokenDto.Token);
            Assert.Equal(DateTime.MinValue, refreshTokenDto.ExpiresAt);
            Assert.False(refreshTokenDto.IsRevoked);
            Assert.False(refreshTokenDto.IsUsed);
            Assert.Equal(DateTime.MinValue, refreshTokenDto.CreatedAt);
            Assert.Null(refreshTokenDto.RevokedAt);
        }

        [Fact]
        public void RefreshTokenDTO_ShouldSetAndGetPropertiesCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "refresh-token-123";
            var expiresAt = DateTime.UtcNow.AddHours(24);
            var isRevoked = true;
            var isUsed = true;
            var createdAt = DateTime.UtcNow;
            var revokedAt = DateTime.UtcNow.AddHours(1);

            var refreshTokenDto = new RefreshTokenDTO
            {
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsRevoked = isRevoked,
                IsUsed = isUsed,
                CreatedAt = createdAt,
                RevokedAt = revokedAt
            };

            // Assert
            Assert.Equal(userId, refreshTokenDto.UserId);
            Assert.Equal(token, refreshTokenDto.Token);
            Assert.Equal(expiresAt, refreshTokenDto.ExpiresAt);
            Assert.Equal(isRevoked, refreshTokenDto.IsRevoked);
            Assert.Equal(isUsed, refreshTokenDto.IsUsed);
            Assert.Equal(createdAt, refreshTokenDto.CreatedAt);
            Assert.Equal(revokedAt, refreshTokenDto.RevokedAt);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void RefreshTokenDTO_ShouldAcceptVariousBooleanValues(bool isRevoked, bool isUsed)
        {
            // Act
            var refreshTokenDto = new RefreshTokenDTO
            {
                IsRevoked = isRevoked,
                IsUsed = isUsed
            };

            // Assert
            Assert.Equal(isRevoked, refreshTokenDto.IsRevoked);
            Assert.Equal(isUsed, refreshTokenDto.IsUsed);
        }

        [Fact]
        public void RefreshTokenDTO_ShouldAllowNullRevokedAt()
        {
            // Act
            var refreshTokenDto = new RefreshTokenDTO
            {
                RevokedAt = null
            };

            // Assert
            Assert.Null(refreshTokenDto.RevokedAt);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("valid-token")]
        [InlineData("another-token-123")]
        [InlineData(null)]
        public void RefreshTokenDTO_ShouldAcceptVariousTokenValues(string? token)
        {
            // Act
            var refreshTokenDto = new RefreshTokenDTO
            {
                Token = token!
            };

            // Assert
            Assert.Equal(token, refreshTokenDto.Token);
        }
    }
}
