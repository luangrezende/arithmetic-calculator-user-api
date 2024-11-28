using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IBankAccountRepository
    {
        Task<IEnumerable<BankAccount>> GetBankAccountsByUserIdAsync(Guid userId);

        Task<bool> AccountExistsAsync(Guid accountId);

        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);

        Task<bool> DebitBalanceAsync(Guid accountId, decimal amount);

        Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId);
    }
}
