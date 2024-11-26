using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly string _connectionString;

        public BankAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<BankAccount> GetBankAccountsByUserId(Guid userId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT 
                        id, 
                        account_type, 
                        balance, 
                        currency 
                    FROM BankAccount 
                    WHERE user_id = @UserId";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = cmd.ExecuteReader();
                var accounts = new List<BankAccount>();

                while (reader.Read())
                {
                    accounts.Add(new BankAccount
                    {
                        Id = reader.GetGuid("id"),
                        AccountType = Enum.Parse<AccountType>(reader.GetString("account_type"), true),
                        Balance = reader.GetDecimal("balance"),
                        Currency = reader.GetString("currency"),
                    });
                }

                return accounts;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

    }
}
