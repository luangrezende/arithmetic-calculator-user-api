using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Domain.Tests.DTOs
{
    public class BankAccountDTOTests
    {
        [Fact]
        public void BankAccountDTO_ShouldInitializeWithDefaultValues()
        {
            // Act
            var bankAccountDto = new BankAccountDTO();

            // Assert
            Assert.Equal(Guid.Empty, bankAccountDto.Id);
            Assert.Equal(Guid.Empty, bankAccountDto.UserId);
            Assert.Equal(0m, bankAccountDto.Balance);
            Assert.Null(bankAccountDto.Currency);
            Assert.Equal(default(DateTime), bankAccountDto.CreatedAt);
            Assert.Null(bankAccountDto.UpdatedAt);
        }

        [Fact]
        public void BankAccountDTO_ShouldSetAndGetPropertiesCorrectly()
        {
            // Arrange
            var id = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var balance = 1000.50m;
            var currency = "USD";
            var createdAt = DateTime.UtcNow;
            var updatedAt = DateTime.UtcNow.AddMinutes(5);

            var bankAccountDto = new BankAccountDTO
            {
                Id = id,
                UserId = userId,
                Balance = balance,
                Currency = currency,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            };

            // Assert
            Assert.Equal(id, bankAccountDto.Id);
            Assert.Equal(userId, bankAccountDto.UserId);
            Assert.Equal(balance, bankAccountDto.Balance);
            Assert.Equal(currency, bankAccountDto.Currency);
            Assert.Equal(createdAt, bankAccountDto.CreatedAt);
            Assert.Equal(updatedAt, bankAccountDto.UpdatedAt);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100.50)]
        [InlineData(-50.25)]
        [InlineData(999999.99)]
        public void BankAccountDTO_ShouldAcceptVariousBalanceValues(decimal balance)
        {
            // Act
            var bankAccountDto = new BankAccountDTO
            {
                Balance = balance
            };

            // Assert
            Assert.Equal(balance, bankAccountDto.Balance);
        }

        [Theory]
        [InlineData("USD")]
        [InlineData("EUR")]
        [InlineData("BRL")]
        [InlineData("")]
        [InlineData(null)]
        public void BankAccountDTO_ShouldAcceptVariousCurrencyValues(string? currency)
        {
            // Act
            var bankAccountDto = new BankAccountDTO
            {
                Currency = currency!
            };

            // Assert
            Assert.Equal(currency, bankAccountDto.Currency);
        }
    }
}
