using MySql.Data.MySqlClient;
using ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services;

namespace ArithmeticCalculatorUserApi.Infrastructure.Data
{
    public class MySqlConnectionService : IDbConnectionService
    {
        private readonly string _connectionString;

        public MySqlConnectionService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<MySqlConnection> CreateConnectionAsync()
        {
            var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<int> ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters, MySqlConnection connection, MySqlTransaction? transaction = null)
        {
            using var cmd = new MySqlCommand(query, connection, transaction);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<object?> ExecuteScalarAsync(string query, Dictionary<string, object> parameters, MySqlConnection connection)
        {
            using var cmd = new MySqlCommand(query, connection);
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value);
            }

            return await cmd.ExecuteScalarAsync();
        }
    }
}
