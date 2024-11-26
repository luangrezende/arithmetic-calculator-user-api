using System.Data;
using ArithmeticCalculatorUserApi.Domain.Enums;
using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public User? Authenticate(string username, string password)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = "SELECT Id, Username, Status FROM User WHERE Username = @Username AND Password = @Password";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Status = reader.GetString("Status"),
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }

            return null;
        }

        public bool UserExists(string username)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = "SELECT COUNT(*) FROM User WHERE Username = @Username";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Username", username);

                var count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public bool CreateUser(string username, string password, string name)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                const string userQuery = "INSERT INTO User (Id, Username, Password, Name) VALUES (@Id, @Username, @Password, @Name)";
                using var userCmd = new MySqlCommand(userQuery, connection, transaction);

                var userId = Guid.NewGuid();
                userCmd.Parameters.AddWithValue("@Id", userId);
                userCmd.Parameters.AddWithValue("@Username", username);
                userCmd.Parameters.AddWithValue("@Password", password); // Substituir por hash em produção.
                userCmd.Parameters.AddWithValue("@Name", name);

                if (userCmd.ExecuteNonQuery() <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                const string accountQuery = "INSERT INTO BankAccount (Id, User_Id, Account_Type, Balance) VALUES (@Id, @UserId, @AccountType, @Balance)";
                using var accountCmd = new MySqlCommand(accountQuery, connection, transaction);

                decimal promotionalAmount = decimal.Parse(Environment.GetEnvironmentVariable("promotionalAmount")!);

                accountCmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                accountCmd.Parameters.AddWithValue("@UserId", userId);
                accountCmd.Parameters.AddWithValue("@AccountType", string.Empty);
                accountCmd.Parameters.AddWithValue("@Balance", promotionalAmount);

                accountCmd.Parameters["@AccountType"].Value = AccountType.Personal.ToString().ToLower();
                if (accountCmd.ExecuteNonQuery() <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                accountCmd.Parameters["@Id"].Value = Guid.NewGuid();
                accountCmd.Parameters["@AccountType"].Value = AccountType.Business.ToString().ToLower();
                if (accountCmd.ExecuteNonQuery() <= 0)
                {
                    transaction.Rollback();
                    return false;
                }

                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public User? GetUserById(Guid userId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = "SELECT id, username, status, name FROM User WHERE id = @UserId";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetGuid("id"),
                        Username = reader.GetString("username"),
                        Status = reader.GetString("status"),
                        Name = reader.GetString("name"),
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }

            return null;
        }
    }
}
