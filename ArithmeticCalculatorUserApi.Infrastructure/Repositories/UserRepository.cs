using System.Data;
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

        public AuthenticateUser? Authenticate(string username, string password)
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
                    return new AuthenticateUser
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

                const string query = "INSERT INTO User (Id, Username, Password, Name) VALUES (@Id, @Username, @Password, @Name)";
                using var cmd = new MySqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@Id", Guid.NewGuid());
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Password", password); // Substituir por hash em produção.
                cmd.Parameters.AddWithValue("@Name", name);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public AuthenticateUser? GetUserById(Guid userId)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();

                const string query = "SELECT Id, Username, Status, Name, Username FROM User WHERE Id = @UserId";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@UserId", userId);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new AuthenticateUser
                    {
                        Id = reader.GetGuid("Id"),
                        Username = reader.GetString("Username"),
                        Status = reader.GetString("Status"),
                        Name = reader.GetString("Name"),
                        Email = reader.GetString("Username"),

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
