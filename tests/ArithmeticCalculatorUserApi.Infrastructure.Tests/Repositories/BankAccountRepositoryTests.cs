using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using ArithmeticCalculatorUserApi.Infrastructure.Persistence;
using Moq;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Tests.Repositories
{
    public class BankAccountRepositoryTests
    {
        private readonly Mock<IDbConnectionService> _dbConnectionServiceMock;
        private readonly BankAccountRepository _repository;

        public BankAccountRepositoryTests()
        {
            _dbConnectionServiceMock = new Mock<IDbConnectionService>();
            _repository = new BankAccountRepository(_dbConnectionServiceMock.Object);
        }

        [Fact]
        public async Task AccountBelongsToUserAsync_ShouldReturnTrue_WhenAccountExists()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            const string query = @"
                    SELECT COUNT(1)
                    FROM bank_account
                    WHERE id = @AccountId AND user_id = @UserId";

            _dbConnectionServiceMock
                .Setup(x => x.ExecuteScalarAsync<int>(query, It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.AccountBelongsToUserAsync(accountId, userId);

            // Assert
            Assert.True(result);
            _dbConnectionServiceMock.VerifyAll();
        }

        [Fact]
        public async Task AddBalanceAsync_ShouldReturnTrue_WhenBalanceIsAddedSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            const decimal amount = 100;

            _dbConnectionServiceMock
                .Setup(x => x.CreateConnectionAsync())
                .ReturnsAsync(new MySqlConnection());

            _dbConnectionServiceMock
                .Setup(x => x.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.AddBalanceAsync(accountId, amount);

            // Assert
            Assert.True(result);
            _dbConnectionServiceMock.VerifyAll();
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnFalse_WhenBalanceIsInsufficient()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            const decimal amount = 100;

            const string checkBalanceQuery = @"
                    SELECT balance 
                    FROM bank_account 
                    WHERE id = @AccountId";

            _dbConnectionServiceMock
                .Setup(x => x.ExecuteScalarAsync<decimal>(checkBalanceQuery, It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .ReturnsAsync(50);

            _dbConnectionServiceMock
                .Setup(x => x.CreateConnectionAsync())
                .ReturnsAsync(new MySqlConnection());

            // Act
            var result = await _repository.DebitBalanceAsync(accountId, amount);

            // Assert
            Assert.False(result);
            _dbConnectionServiceMock.VerifyAll();
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnTrue_WhenBalanceIsSufficient()
        {
            // Arrange
            _dbConnectionServiceMock
                .Setup(x => x.ExecuteScalarAsync<decimal>(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), null))
                .ReturnsAsync(100); // Mock balance

            _dbConnectionServiceMock
                .Setup(x => x.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), null))
                .ReturnsAsync(1); // Simulate successful debit

            // Act
            var result = await _repository.DebitBalanceAsync(Guid.NewGuid(), 50);

            // Assert
            Assert.True(result);
            _dbConnectionServiceMock.VerifyAll();
        }

        [Fact]
        public async Task GetBankAccountsByUserIdAsync_ShouldReturnAccounts_WhenAccountsExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const string query = @"
                    SELECT 
                        id, 
                        balance, 
                        currency 
                    FROM bank_account 
                    WHERE user_id = @UserId";

            var accounts = new List<BankAccountEntity>
            {
                new BankAccountEntity { Id = Guid.NewGuid(), Balance = 100, Currency = "USD" },
                new BankAccountEntity { Id = Guid.NewGuid(), Balance = 200, Currency = "EUR" }
            };

            _dbConnectionServiceMock
                .Setup(x => x.CreateConnectionAsync())
                .ReturnsAsync(new MySqlConnection());

            // Act
            var result = await _repository.GetBankAccountsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            _dbConnectionServiceMock.VerifyAll();
        }

        [Fact]
        public async Task CreateBankAccountAsync_ShouldReturnTrue_WhenAccountIsCreatedSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const decimal initialBalance = 100;

            _dbConnectionServiceMock
                .Setup(x => x.ExecuteNonQueryAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<MySqlConnection>(), It.IsAny<MySqlTransaction>()))
                .ReturnsAsync(1);

            // Act
            var result = await _repository.CreateBankAccountAsync(userId, initialBalance, new MySqlConnection(), null);

            // Assert
            Assert.True(result);
            _dbConnectionServiceMock.VerifyAll();
        }
    }
}
