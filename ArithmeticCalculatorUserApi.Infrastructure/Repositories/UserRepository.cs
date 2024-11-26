using System.Data;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Infrastructure.Security;
using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        private readonly decimal _promotionalAmount;

        public UserRepository()
        {
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString")!;
            _promotionalAmount = decimal.Parse(Environment.GetEnvironmentVariable("promotionalAmount")!);
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            const string query = "SELECT Id, Username, Password, Name, Status FROM User WHERE Username = @Username";
            var user = await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            });

            if (user == null || !PasswordHasher.VerifyPassword(password, user.Password!))
                return null;

            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            const string query = "SELECT COUNT(1) FROM User WHERE Username = @Username";
            return await ExecuteScalarAsync(query, new Dictionary<string, object>
            {
                { "@Username", username }
            }) > 0;
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var transaction = await connection.BeginTransactionAsync();

                var userId = Guid.NewGuid();
                var hashedPassword = PasswordHasher.HashPassword(password);

                const string userQuery = "INSERT INTO User (Id, Username, Password, Name) VALUES (@Id, @Username, @Password, @Name)";
                if (await ExecuteNonQueryAsync(userQuery, new Dictionary<string, object>
                {
                    { "@Id", userId },
                    { "@Username", username },
                    { "@Password", hashedPassword },
                    { "@Name", name }
                }, connection, transaction) <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                const string accountQuery = "INSERT INTO BankAccount (Id, User_Id, Account_Type, Balance) VALUES (@Id, @UserId, @AccountType, @Balance)";
                if (await ExecuteNonQueryAsync(accountQuery, new Dictionary<string, object>
                {
                    { "@Id", Guid.NewGuid() },
                    { "@UserId", userId },
                    { "@AccountType", AccountType.Personal.ToString().ToLower() },
                    { "@Balance", _promotionalAmount }
                }, connection, transaction) <= 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            const string query = "SELECT Id, Username, Name, Status FROM User WHERE Id = @UserId";
            return await GetUserFromQueryAsync(query, new Dictionary<string, object>
            {
                { "@UserId", userId }
            });
        }

        private async Task<User?> GetUserFromQueryAsync(string query, Dictionary<string, object> parameters)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new MySqlCommand(query, connection);
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                using var reader = await cmd.ExecuteReaderAsync();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Name = reader.GetString("Name"),
                        Status = reader.GetString("Status"),
                        Password = reader["Password"] as string
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }

            return null;
        }

        private async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters, MySqlConnection? connection = null, MySqlTransaction? transaction = null)
        {
            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> ExecuteScalarAsync(string query, Dictionary<string, object> parameters, MySqlConnection? connection = null)
        {
            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
    }
}
