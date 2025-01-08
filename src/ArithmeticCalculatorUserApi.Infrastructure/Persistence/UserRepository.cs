using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using ArithmeticCalculatorUserApi.Infrastructure.Extensions;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Persistence
{
    public class UserRepository : IUserRepository
    {
        private readonly decimal _PROMOTIONAL_AMOUNT;
        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IDbConnectionService _dbConnectionService;
        private readonly ISecurityService _securityService;

        public UserRepository(
            IBankAccountRepository bankAccountRepository, 
            IDbConnectionService dbConnectionService,
            ISecurityService securityService)
        {
            _bankAccountRepository = bankAccountRepository;
            _dbConnectionService = dbConnectionService;
            _securityService = securityService;
            _PROMOTIONAL_AMOUNT = decimal.Parse(Environment.GetEnvironmentVariable("PROMOTIONAL_AMOUNT") ?? throw new InvalidOperationException("Promotional amount is not set."));
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                var userId = Guid.NewGuid();
                var hashedPassword = _securityService.HashPassword(password);

                const string userQuery = @"
                    INSERT INTO user (id, username, password, name, user_status_id) 
                    VALUES (@Id, @Username, @Password, @Name, 1)";

                var userInserted = await _dbConnectionService.ExecuteNonQueryAsync(userQuery, new Dictionary<string, object>
                {
                    { "@Id", userId },
                    { "@Username", username },
                    { "@Password", hashedPassword },
                    { "@Name", name }
                }, connection, transaction);

                if (userInserted <= 0)
                    throw new Exception("Failed to insert user.");

                var accountCreated = await _bankAccountRepository.CreateBankAccountAsync(userId, _PROMOTIONAL_AMOUNT, connection, transaction);
                if (!accountCreated)
                    throw new Exception("Failed to create bank account.");

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<UserEntity?> GetUserByIdAsync(Guid userId)
        {
            const string query = @"
                SELECT u.id, u.username, u.name, us.description AS status 
                FROM user u
                INNER JOIN user_status us ON u.user_status_id = us.id
                WHERE u.id = @UserId";

            return await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@UserId", userId }
            });
        }

        public async Task<UserEntity?> GetUserByUsernameAsync(string username)
        {
            const string query = @"
                SELECT u.id, u.username, u.password, u.name, us.description AS status 
                FROM user u
                INNER JOIN user_status us ON u.user_status_id = us.id
                WHERE u.username = @Username";

            return await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });
        }

        private async Task<UserEntity?> GetUserFromQueryAsync(string query, Dictionary<string, object> parameters)
        {
            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return new UserEntity
                {
                    Id = reader.GetGuid("id"),
                    Username = reader.GetString("username"),
                    Name = reader.GetString("name"),
                    Status = reader.GetString("status"),
                    Password = reader.HasColumn("password") ? reader["password"] as string : null
                };
            }

            return null;
        }
    }
}
