using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly string _connectionString;

        public BankAccountRepository()
        {
            var connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString");
            _connectionString = connectionString!;
        }

        public async Task<bool> AccountBelongsToUserAsync(string accountId, Guid userId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT COUNT(1)
                    FROM BankAccount
                    WHERE id = @AccountId AND user_id = @UserId";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AccountId", accountId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                var result = await cmd.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<bool> AccountExistsAsync(string accountId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = @"
                    SELECT COUNT(1)
                    FROM BankAccount
                    WHERE id = @AccountId";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@AccountId", accountId);

                var result = await cmd.ExecuteScalarAsync();

                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<bool> AddBalanceAsync(string accountId, decimal amount)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    UPDATE BankAccount
                    SET balance = balance + @Amount,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE id = @AccountId";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Amount", amount);
                cmd.Parameters.AddWithValue("@AccountId", accountId);

                var rowsAffected = await cmd.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<bool> DebitBalanceAsync(string accountId, decimal amount)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string checkBalanceQuery = @"
                    SELECT balance 
                    FROM BankAccount 
                    WHERE id = @AccountId";

                using var checkBalanceCmd = new MySqlCommand(checkBalanceQuery, connection);
                checkBalanceCmd.Parameters.AddWithValue("@AccountId", accountId);

                var balanceObj = await checkBalanceCmd.ExecuteScalarAsync();
                if (balanceObj == null)
                {
                    return false;
                }

                var balance = Convert.ToDecimal(balanceObj);
                if (balance < amount)
                {
                    return false;
                }

                const string debitQuery = @"
                    UPDATE BankAccount
                    SET balance = balance - @Amount,
                        updated_at = CURRENT_TIMESTAMP
                    WHERE id = @AccountId";

                using var debitCmd = new MySqlCommand(debitQuery, connection);
                debitCmd.Parameters.AddWithValue("@Amount", amount);
                debitCmd.Parameters.AddWithValue("@AccountId", accountId);

                var rowsAffected = await debitCmd.ExecuteNonQueryAsync();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<IEnumerable<BankAccount>> GetBankAccountsByUserIdAsync(Guid userId)
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

                using var reader = await cmd.ExecuteReaderAsync();
                var accounts = new List<BankAccount>();

                while (await reader.ReadAsync())
                {
                    accounts.Add(new BankAccount
                    {
                        Id = reader.GetGuid("id"),
                        AccountType = reader.GetString("account_type"),
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
