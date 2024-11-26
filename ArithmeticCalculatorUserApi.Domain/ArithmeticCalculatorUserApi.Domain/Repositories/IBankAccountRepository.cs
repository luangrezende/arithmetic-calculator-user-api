using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IBankAccountRepository
    {
        IEnumerable<BankAccount> GetBankAccountsByUserId(Guid userId);
    }
}
