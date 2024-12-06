using ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services;
using ArithmeticCalculatorUserApi.Infrastructure.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private readonly IDbConnectionService _dbConnectionService;

        public BankAccountRepository(IDbConnectionService dbConnectionService)
        {
            _dbConnectionService = dbConnectionService;
        }

        public async Task<bool> AccountBelongsToUserAsync(Guid accountId, Guid userId)
        {
            const string query = @"
                SELECT COUNT(1)
                FROM bank_account
                WHERE id = @AccountId AND user_id = @UserId";

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            var result = await _dbConnectionService.ExecuteScalarAsync<int>(query, new Dictionary<string, object>
            {
                { "@AccountId", accountId },
                { "@UserId", userId }
            }, connection);

            return result > 0;
        }

        public async Task<bool> AddBalanceAsync(Guid accountId, decimal amount)
        {
            const string updateQuery = @"
                UPDATE bank_account
                SET balance = balance + @Amount,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @AccountId";

            const string insertRecordQuery = @"
                INSERT INTO balance_record (id, account_id, amount, type)
                VALUES (@Id, @AccountId, @Amount, 'credit')";

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            var result = await _dbConnectionService.ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection, transaction);

            if (result == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await _dbConnectionService.ExecuteNonQueryAsync(insertRecordQuery, new Dictionary<string, object>
            {
                { "@Id", Guid.NewGuid() },
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection, transaction);

            await transaction.CommitAsync();
            return true;
        }

        public async Task<bool> DebitBalanceAsync(Guid accountId, decimal amount)
        {
            const string checkBalanceQuery = @"
                SELECT balance 
                FROM bank_account 
                WHERE id = @AccountId";

            const string updateQuery = @"
                UPDATE bank_account
                SET balance = balance - @Amount,
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @AccountId";

            const string insertRecordQuery = @"
                INSERT INTO balance_record (id, account_id, amount, type)
                VALUES (@Id, @AccountId, @Amount, 'debit')";

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            var balance = await _dbConnectionService.ExecuteScalarAsync<decimal>(checkBalanceQuery, new Dictionary<string, object>
            {
                { "@AccountId", accountId }
            }, connection, transaction);

            if (balance < amount)
            {
                await transaction.RollbackAsync();
                return false;
            }

            var result = await _dbConnectionService.ExecuteNonQueryAsync(updateQuery, new Dictionary<string, object>
            {
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection, transaction);

            if (result == 0)
            {
                await transaction.RollbackAsync();
                return false;
            }

            await _dbConnectionService.ExecuteNonQueryAsync(insertRecordQuery, new Dictionary<string, object>
            {
                { "@Id", Guid.NewGuid() },
                { "@Amount", amount },
                { "@AccountId", accountId }
            }, connection, transaction);

            await transaction.CommitAsync();
            return true;
        }

        public async Task<IEnumerable<BankAccountEntity>> GetBankAccountsByUserIdAsync(Guid userId)
        {
            const string query = @"
                SELECT 
                    id, 
                    balance, 
                    currency 
                FROM bank_account 
                WHERE user_id = @UserId";

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@UserId", userId);

            using var reader = await cmd.ExecuteReaderAsync();
            var accounts = new List<BankAccountEntity>();

            while (await reader.ReadAsync())
            {
                accounts.Add(new BankAccountEntity
                {
                    Id = reader.GetGuid("id"),
                    Balance = reader.GetDecimal("balance"),
                    Currency = reader.GetString("currency"),
                });
            }

            return accounts;
        }

        public async Task<bool> CreateBankAccountAsync(Guid userId, decimal initialBalance, MySqlConnection connection, MySqlTransaction transaction)
        {
            const string query = @"
                INSERT INTO bank_account (id, user_id, balance) 
                VALUES (@Id, @UserId, @Balance)";

            var parameters = new Dictionary<string, object>
            {
                { "@Id", Guid.NewGuid() },
                { "@UserId", userId },
                { "@Balance", initialBalance }
            };

            var result = await _dbConnectionService.ExecuteNonQueryAsync(query, parameters, connection, transaction);

            return result > 0;
        }
    }
}
