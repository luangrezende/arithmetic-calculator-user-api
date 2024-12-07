using ArithmeticCalculatorUserApi.Application.DTOs;
using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Application.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using Moq;

namespace ArithmeticCalculatorUserApi.Domain.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly Mock<ISecurityService> _mockSecurityService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _mockSecurityService = new Mock<ISecurityService>();
            _userService = new UserService(_mockUserRepository.Object, _mockSecurityService.Object);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnUserDTO_WhenCredentialsAreValid()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new UserDTO
            {
                Id = Guid.NewGuid(),
                Username = username,
                Status = "active",
                Name = "Test User"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(username))
                .ReturnsAsync(new UserEntity
                {
                    Id = user.Id,
                    Username = user.Username,
                    Password = hashedPassword,
                    Status = "active",
                    Name = "Test User"
                });

            _mockUserRepository.Setup(repo => repo.CreateUserAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result?.Username);
            Assert.Equal(user.Name, result?.Name);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnNull_WhenCredentialsAreInvalid()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";

            _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(username))
                .ReturnsAsync((UserEntity)null!);

            // Act
            var result = await _userService.AuthenticateAsync(username, password);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnTrue_WhenUserIsCreatedSuccessfully()
        {
            // Arrange
            var username = "newuser";
            var password = "password123";
            var name = "New User";

            _mockUserRepository.Setup(repo => repo.CreateUserAsync(username, password, name))
                .ReturnsAsync(true);

            // Act
            var result = await _userService.CreateUserAsync(username, password, name);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldReturnFalse_WhenUserCreationFails()
        {
            // Arrange
            var username = "newuser";
            var password = "password123";
            var name = "New User";

            _mockUserRepository.Setup(repo => repo.CreateUserAsync(username, password, name))
                .ReturnsAsync(false);

            // Act
            var result = await _userService.CreateUserAsync(username, password, name);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserDTO_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new UserDTO
            {
                Id = userId,
                Username = "testuser",
                Status = "active",
                Name = "Test User"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync(new UserEntity { Id = userId, Username = user.Username, Status = user.Status, Name = user.Name });

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result?.Username);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                .ReturnsAsync((UserEntity?)null);

            // Act
            var result = await _userService.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnUserDTO_WhenUserExists()
        {
            // Arrange
            var username = "testuser";
            var user = new UserDTO
            {
                Id = Guid.NewGuid(),
                Username = username,
                Status = "active",
                Name = "Test User"
            };

            _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(username))
                .ReturnsAsync(new UserEntity { Id = user.Id, Username = username, Status = user.Status, Name = user.Name });

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Username, result?.Username);
        }

        [Fact]
        public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var username = "testuser";

            _mockUserRepository.Setup(repo => repo.GetUserByUsernameAsync(username))
                .ReturnsAsync((UserEntity?)null);

            // Act
            var result = await _userService.GetUserByUsernameAsync(username);

            // Assert
            Assert.Null(result);
        }
    }
}
