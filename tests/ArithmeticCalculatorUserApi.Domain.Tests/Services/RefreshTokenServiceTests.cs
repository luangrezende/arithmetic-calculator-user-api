using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using Moq;

namespace ArithmeticCalculatorUserApi.Domain.Tests.Services
{
    public class RefreshTokenServiceTests
    {
        private readonly Mock<IRefreshTokenRepository> _mockRepository;
        private readonly RefreshTokenService _service;

        public RefreshTokenServiceTests()
        {
            _mockRepository = new Mock<IRefreshTokenRepository>();
            _service = new RefreshTokenService(_mockRepository.Object);
        }

        [Fact]
        public async Task AddAsync_ShouldReturnToken_WhenRefreshTokenIsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newRefreshToken = new RefreshTokenEntity
            {
                UserId = userId,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                Token = "generatedToken"
            };

            _mockRepository.Setup(repo => repo.AddAsync(It.IsAny<RefreshTokenEntity>()))
                .ReturnsAsync(newRefreshToken);

            // Act
            var result = await _service.AddAsync(userId);

            // Assert
            Assert.Equal("generatedToken", result);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnRefreshTokenDTO_WhenTokenIsValid()
        {
            // Arrange
            var token = "validToken";
            var refreshTokenEntity = new RefreshTokenEntity
            {
                Token = token,
                UserId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
                IsUsed = false,
                RevokedAt = null
            };
            _mockRepository.Setup(repo => repo.GetByTokenAsync(token))
                .ReturnsAsync(refreshTokenEntity);

            // Act
            var result = await _service.GetByTokenAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(token, result.Token);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnNull_WhenTokenIsNotFound()
        {
            // Arrange
            var token = "invalidToken";
            _mockRepository.Setup(repo => repo.GetByTokenAsync(token))
                .ReturnsAsync((RefreshTokenEntity?)null);

            // Act
            var result = await _service.GetByTokenAsync(token);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task InvalidateTokenAsync_ShouldReturnTrue_WhenTokenIsSuccessfullyInvalidated()
        {
            // Arrange
            var token = "validToken";
            _mockRepository.Setup(repo => repo.InvalidateTokenAsync(token))
                .ReturnsAsync(true);

            // Act
            var result = await _service.InvalidateTokenAsync(token);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task InvalidateTokenAsync_ShouldReturnFalse_WhenTokenInvalidationFails()
        {
            // Arrange
            var token = "invalidToken";
            _mockRepository.Setup(repo => repo.InvalidateTokenAsync(token))
                .ReturnsAsync(false);

            // Act
            var result = await _service.InvalidateTokenAsync(token);

            // Assert
            Assert.False(result);
        }
    }
}
