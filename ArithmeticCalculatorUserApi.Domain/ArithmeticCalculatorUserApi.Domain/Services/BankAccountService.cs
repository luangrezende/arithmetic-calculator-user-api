using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;

namespace ArithmeticCalculatorUserApi.Domain.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly IBankAccountRepository _bankAccountRepository;

        public BankAccountService(IBankAccountRepository bankAccountRepository)
        {
            _bankAccountRepository = bankAccountRepository;
        }

        public async Task<bool> AccountBelongsToUserAsync(string accountId, Guid userId)
        {
            return await _bankAccountRepository.AccountBelongsToUserAsync(accountId, userId);
        }

        public async Task<bool> AccountExistsAsync(string accountId)
        {
            return await _bankAccountRepository.AccountExistsAsync(accountId);
        }

        public async Task<bool> AddBalanceAsync(string accountId, decimal amount)
        {
            return await _bankAccountRepository.AddBalanceAsync(accountId, amount);
        }

        public async Task<bool> DebitBalanceAsync(string accountId, decimal amount)
        {
            return await _bankAccountRepository.DebitBalanceAsync(accountId, amount);
        }

        public async Task<IEnumerable<BankAccount>> GetBankAccountsByUserIdAsync(Guid userId)
        {
            return await _bankAccountRepository.GetBankAccountsByUserIdAsync(userId);
        }
    }
}
