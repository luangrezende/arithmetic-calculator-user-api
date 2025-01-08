using Moq;
using ArithmeticCalculatorUserApi.Domain.Entities;
using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Services;
using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Domain.Tests.Services
{
    public class BankAccountServiceTests
    {
        private readonly Mock<IBankAccountRepository> _mockRepository;
        private readonly BankAccountService _service;

        public BankAccountServiceTests()
        {
            _mockRepository = new Mock<IBankAccountRepository>();
            _service = new BankAccountService(_mockRepository.Object);
        }

        [Fact]
        public async Task AccountBelongsToUserAsync_ShouldReturnTrue_WhenAccountBelongsToUser()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.AccountBelongsToUserAsync(accountId, userId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AccountBelongsToUserAsync(accountId, userId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AccountBelongsToUserAsync_ShouldReturnFalse_WhenAccountDoesNotBelongToUser()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            _mockRepository.Setup(repo => repo.AccountBelongsToUserAsync(accountId, userId))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AccountBelongsToUserAsync(accountId, userId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddBalanceAsync_ShouldReturnTrue_WhenBalanceAddedSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            decimal amount = 100m;
            _mockRepository.Setup(repo => repo.AddBalanceAsync(accountId, amount))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddBalanceAsync(accountId, amount);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddBalanceAsync_ShouldReturnFalse_WhenBalanceNotAdded()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            decimal amount = 100m;
            _mockRepository.Setup(repo => repo.AddBalanceAsync(accountId, amount))
                .ReturnsAsync(false);

            // Act
            var result = await _service.AddBalanceAsync(accountId, amount);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnTrue_WhenBalanceDebitedSuccessfully()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            decimal amount = 50m;
            _mockRepository.Setup(repo => repo.DebitBalanceAsync(accountId, amount))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DebitBalanceAsync(accountId, amount);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DebitBalanceAsync_ShouldReturnFalse_WhenBalanceNotDebited()
        {
            // Arrange
            var accountId = Guid.NewGuid();
            decimal amount = 50m;
            _mockRepository.Setup(repo => repo.DebitBalanceAsync(accountId, amount))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DebitBalanceAsync(accountId, amount);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetBankAccountsByUserIdAsync_ShouldReturnListOfBankAccounts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var accounts = new List<BankAccountDTO>
            {
                new BankAccountDTO
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Balance = 100m,
                    Currency = "USD",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            _mockRepository.Setup(repo => repo.GetBankAccountsByUserIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<BankAccountEntity>
                {
                    new BankAccountEntity
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        Balance = 100m,
                        Currency = "USD",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                }.AsEnumerable());

            // Act
            var result = await _service.GetBankAccountsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userId, result.First().UserId);
        }

        [Fact]
        public async Task GetBankAccountsByUserIdAsync_ShouldReturnEmptyList_WhenNoAccountsFound()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _mockRepository.Setup(repo => repo.GetBankAccountsByUserIdAsync(It.IsAny<Guid>()))
                 .ReturnsAsync([]);

            // Act
            var result = await _service.GetBankAccountsByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
