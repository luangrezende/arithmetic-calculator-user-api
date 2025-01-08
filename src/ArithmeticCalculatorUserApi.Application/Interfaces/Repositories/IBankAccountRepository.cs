using ArithmeticCalculatorUserApi.Domain.Entities;
using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Application.Interfaces.Repositories
{
    public interface IBankAccountRepository
    {
        Task<IEnumerable<BankAccountEntity>> GetBankAccountsByUserIdAsync(Guid userId);

        Task<bool> AddBalanceAsync(Guid accountId, decimal amount);

        Task<bool> DebitBalanceAsync(Guid accountId, decimal amount);

        Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId);

        Task<bool> CreateBankAccountAsync(
            Guid userId,
            decimal initialBalance,
            MySqlConnection connection,
            MySqlTransaction transaction);
    }
}
