using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Domain.Tests.DTOs
{
    public class UserDTOTests
    {
        [Theory]
        [InlineData("Active", true)]
        [InlineData("active", true)]
        [InlineData("ACTIVE", true)]
        [InlineData("Inactive", false)]
        [InlineData("inactive", false)]
        [InlineData("Pending", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsActive_ShouldReturnCorrectValue_BasedOnStatus(string? status, bool expected)
        {
            // Arrange
            var userDto = new UserDTO
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Name = "Test User",
                Status = status!
            };

            // Act
            var result = userDto.IsActive();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void UserDTO_ShouldInitializeWithDefaultValues()
        {
            // Act
            var userDto = new UserDTO();

            // Assert
            Assert.Equal(Guid.Empty, userDto.Id);
            Assert.Null(userDto.Username);
            Assert.Null(userDto.Name);
            Assert.Null(userDto.Status);
            Assert.Null(userDto.Accounts);
        }

        [Fact]
        public void UserDTO_ShouldSetAndGetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var username = "testuser";
            var name = "Test User";
            var status = "Active";
            var accounts = new List<BankAccountDTO>();

            var userDto = new UserDTO
            {
                Id = id,
                Username = username,
                Name = name,
                Status = status,
                Accounts = accounts
            };

            // Assert
            Assert.Equal(id, userDto.Id);
            Assert.Equal(username, userDto.Username);
            Assert.Equal(name, userDto.Name);
            Assert.Equal(status, userDto.Status);
            Assert.Equal(accounts, userDto.Accounts);
        }
    }
}
