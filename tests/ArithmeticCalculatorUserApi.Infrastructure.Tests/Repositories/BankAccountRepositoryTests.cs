using Moq;
using ArithmeticCalculatorUserApi.Infrastructure.Repositories;
using ArithmeticCalculatorUserApi.Infrastructure.Models;
using MySql.Data.MySqlClient;
using ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services;

namespace ArithmeticCalculatorUserApi.Infrastructure.Tests.Repositories
{
    public class BankAccountRepositoryTests
    {
        private readonly Mock<IDbConnectionService> _mockDbConnectionService;
        private readonly BankAccountRepository _bankAccountRepository;

        public BankAccountRepositoryTests()
        {
            _mockDbConnectionService = new Mock<IDbConnectionService>();
            _bankAccountRepository = new BankAccountRepository(_mockDbConnectionService.Object);
        }

        [Fact]
        public async Task AccountBelongsToUserAsync_ShouldReturnTrue_WhenAccountBelongsToUser()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var mockParameters = new Dictionary<string, object>
            {
                { "@AccountId", accountId },
                { "@UserId", userId }
            };

            _mockDbConnectionService
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>()))
                .ReturnsAsync(1);

            // Act
            var result = await _bankAccountRepository.AccountBelongsToUserAsync(accountId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AccountBelongsToUserAsync_ShouldReturnFalse_WhenAccountDoesNotBelongToUser()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var mockQuery = "SELECT COUNT(1) FROM bank_account WHERE id = @AccountId AND user_id = @UserId";
            var mockParameters = new Dictionary<string, object>
            {
                { "@AccountId", accountId },
                { "@UserId", userId }
            };

            // Setup mock
            _mockDbConnectionService
                .Setup(x => x.ExecuteScalarAsync(mockQuery, mockParameters, It.IsAny<MySqlConnection>()))
                .ReturnsAsync(0);

            // Act
            var result = await _bankAccountRepository.AccountBelongsToUserAsync(accountId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddBalanceAsync_ShouldReturnTrue_WhenBalanceIsAdded()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var amount = 100m;

            var mockParameters = new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            };

            _mockDbConnectionService
                 .Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>()))
                 .ReturnsAsync(1);

            // Act
            var result = await _bankAccountRepository.AddBalanceAsync(accountId, amount);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnFalse_WhenInsufficientBalance()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var amount = 100m;

            _mockDbConnectionService
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>()))
                .ReturnsAsync(50m);

            // Act
            var result = await _bankAccountRepository.DebitBalanceAsync(accountId, amount);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnTrue_WhenBalanceIsDebited()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var amount = 100m;

            _mockDbConnectionService
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>()))
                .ReturnsAsync(200m);

            var mockQuery = "UPDATE bank_account SET balance = balance - @Amount, updated_at = CURRENT_TIMESTAMP WHERE id = @AccountId";
            var mockParameters = new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            };

            _mockDbConnectionService
                .Setup(x => x.ExecuteNonQueryAsync(mockQuery, mockParameters, It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .ReturnsAsync(1);

            // Act
            var result = await _bankAccountRepository.DebitBalanceAsync(accountId, amount);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetBankAccountsByUserIdAsync_ShouldReturnBankAccounts_WhenAccountsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var mockParameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            var mockData = new List<BankAccountEntity>
            {
                new() { Id = Guid.NewGuid(), Balance = 100m, Currency = "USD" },
                new() { Id = Guid.NewGuid(), Balance = 200m, Currency = "EUR" }
            };

            _mockDbConnectionService
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>()))
                .ReturnsAsync(mockData);

            // Act
            var result = await _bankAccountRepository.GetBankAccountsByUserIdAsync(userId);

            // Assert
            Assert.NotEmpty(result);
        }
    }
}
