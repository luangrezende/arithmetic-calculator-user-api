using System;
using System.Data;
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

        public (int userId, string username, string status)? Authenticate(string username, string password)
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
                    return (reader.GetInt32("Id"), reader.GetString("Username"), reader.GetString("Status"));
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

                const string query = "INSERT INTO User (Username, Password, Name) VALUES (@Username, @Password, @Name)";
                using var cmd = new MySqlCommand(query, connection);
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

    }
}
