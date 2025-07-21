using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Infrastructure.Persistence;
using Moq;
using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Infrastructure.Tests.Repositories
{
    public class SimpleBankAccountRepositoryTests
    {
        private readonly Mock<IDbConnectionService> _dbConnectionServiceMock;
        private readonly BankAccountRepository _repository;

        public SimpleBankAccountRepositoryTests()
        {
            _dbConnectionServiceMock = new Mock<IDbConnectionService>();
            _repository = new BankAccountRepository(_dbConnectionServiceMock.Object);
        }

        [Theory]
        [InlineData(-100)]
        [InlineData(0)]
        public async Task AddBalanceAsync_ShouldThrowArgumentException_WhenAmountIsInvalid(decimal amount)
        {
            // Arrange
            var accountId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.AddBalanceAsync(accountId, amount));
        }

        [Theory]
        [InlineData(-50)]
        [InlineData(0)]
        public async Task DebitBalanceAsync_ShouldThrowArgumentException_WhenAmountIsInvalid(decimal amount)
        {
            // Arrange
            var accountId = Guid.NewGuid();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.DebitBalanceAsync(accountId, amount));
        }

        [Fact]
        public async Task CreateBankAccountAsync_ShouldThrowArgumentException_WhenInitialBalanceIsNegative()
        {
            // Arrange
            var userId = Guid.NewGuid();
            const decimal initialBalance = -100;
            var connection = new MySqlConnection("Server=localhost");
            var transaction = default(MySqlTransaction)!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => 
                _repository.CreateBankAccountAsync(userId, initialBalance, connection, transaction));
        }
    }
}
