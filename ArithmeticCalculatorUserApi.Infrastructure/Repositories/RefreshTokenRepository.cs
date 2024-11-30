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
            _connectionString = Environment.GetEnvironmentVariable("mysqlConnectionString")
                                ?? throw new InvalidOperationException("Connection string is not set.");
        }

        public async Task<bool> AddAsync(RefreshToken refreshToken)
        {
            const string query = @"
                INSERT INTO refresh_tokens (token, user_id, expires_at, created_at, is_revoked)
                VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked)";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", refreshToken.Token);
                cmd.Parameters.AddWithValue("@UserId", refreshToken.UserId);
                cmd.Parameters.AddWithValue("@ExpiresAt", refreshToken.ExpiresAt);
                cmd.Parameters.AddWithValue("@CreatedAt", refreshToken.CreatedAt);
                cmd.Parameters.AddWithValue("@IsRevoked", refreshToken.IsRevoked);

                return await cmd.ExecuteNonQueryAsync() > 0;
            }
            catch (Exception ex)
            {
                throw new DataException("Error while adding a refresh token to the database.", ex);
            }
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            const string query = @"
                SELECT token, user_id, expires_at, created_at, is_revoked 
                FROM refresh_tokens
                WHERE token = @Token AND is_revoked = 0";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Token", token);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new RefreshToken
                    {
                        Token = reader.GetString("token"),
                        UserId = reader.GetGuid("user_id"),
                        ExpiresAt = reader.GetDateTime("expires_at"),
                        CreatedAt = reader.GetDateTime("created_at"),
                        IsRevoked = reader.GetBoolean("is_revoked")
                    };
                }
            }
            catch (Exception ex)
            {
                throw new DataException("Error while retrieving a refresh token from the database.", ex);
            }

            return null;
        }

        public async Task InvalidateTokenAsync(string token)
        {
            const string query = @"
                UPDATE refresh_tokens
                SET is_revoked = 1, 
                    revoked_at = @RevokedAt
                WHERE token = @Token";

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@RevokedAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@Token", token);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new DataException("Error while invalidating a refresh token in the database.", ex);
            }
        }
    }
}
