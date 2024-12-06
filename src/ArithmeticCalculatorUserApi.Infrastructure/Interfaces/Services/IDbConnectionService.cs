using MySql.Data.MySqlClient;

namespace ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services
{
    public interface IDbConnectionService
    {
        Task<int> ExecuteNonQueryAsync(
            string query, 
            Dictionary<string, object> parameters, 
            MySqlConnection connection, 
            MySqlTransaction? transaction = null);

        Task<T?> ExecuteScalarAsync<T>(
             string query,
             Dictionary<string, object> parameters,
             MySqlConnection connection,
             MySqlTransaction? transaction = null);

        Task<MySqlConnection> CreateConnectionAsync();
    }
}
