using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using MySql.Data.MySqlClient;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly string _connectionString;

        public RefreshTokenRepository()
        {
            var connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString");
            _connectionString = connectionString!;
        }

        public async Task<bool> AddAsync(RefreshToken refreshToken)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    INSERT INTO RefreshTokens (Token, UserId, ExpiresAt, CreatedAt, IsUsed, IsRevoked)
                    VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsUsed, @IsRevoked)";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", refreshToken.Token);
                cmd.Parameters.AddWithValue("@UserId", refreshToken.UserId);
                cmd.Parameters.AddWithValue("@ExpiresAt", refreshToken.ExpiresAt);
                cmd.Parameters.AddWithValue("@CreatedAt", refreshToken.CreatedAt);
                cmd.Parameters.AddWithValue("@IsUsed", refreshToken.IsUsed);
                cmd.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    SELECT Token, UserId, ExpiresAt, CreatedAt, IsUsed, IsRevoked
                    FROM RefreshTokens
                    WHERE Token = @Token AND IsUsed = 0 AND IsRevoked = 0";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", token);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RefreshToken
                    {
                        Token = reader.GetString("Token"),
                        UserId = reader.GetGuid("UserId"),
                        ExpiresAt = reader.GetDateTime("ExpiresAt"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        IsUsed = reader.GetBoolean("IsUsed"),
                        IsRevoked = reader.GetBoolean("IsRevoked")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }

            return null;
        }

        public async Task InvalidateTokenAsync(string token)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                    UPDATE RefreshTokens
                    SET IsRevoked = 1, 
                        RevokedAt = @RevokedAt
                    WHERE Token = @Token";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RevokedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Token", token);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new DataException("Error during database operation.", ex);
            }
        }
    }
}
