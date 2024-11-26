using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Services.Interfaces
{
    public interface IBankAccountService
    {
        Task<IEnumerable<BankAccount>> GetBankAccountsByUserIdAsync(Guid userId);

        Task<bool> AccountExistsAsync(string accountId);

        Task<bool> AddBalanceAsync(string accountId, decimal amount);

        Task<bool> DebitBalanceAsync(string accountId, decimal amount);

        Task<bool> AccountBelongsToUserAsync(string accountId, Guid userId);
    }
}
