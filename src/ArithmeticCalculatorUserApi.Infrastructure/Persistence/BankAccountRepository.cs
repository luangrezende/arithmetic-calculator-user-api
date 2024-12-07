using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Persistence
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
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

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

            try
            {
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DebitBalanceAsync(Guid accountId, decimal amount)
        {
            if (amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

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

            try
            {
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
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
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

            var parameters = new Dictionary<string, object>
            {
                { "@UserId", userId }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var reader = await _dbConnectionService.ExecuteReaderAsync(query, parameters, connection);

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
            if (initialBalance < 0)
                throw new ArgumentException("Initial balance cannot be negative.");

            const string query = @"
                INSERT INTO bank_account (id, user_id, balance, created_at, updated_at) 
                VALUES (@Id, @UserId, @Balance, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

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
