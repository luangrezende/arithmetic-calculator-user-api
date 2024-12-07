using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;
using ArithmeticCalculatorUserApi.Domain.Entities;
using System.Data;

namespace ArithmeticCalculatorUserApi.Infrastructure.Persistence
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IDbConnectionService _dbConnectionService;

        public RefreshTokenRepository(IDbConnectionService dbConnectionService)
        {
            _dbConnectionService = dbConnectionService;
        }

        public async Task<RefreshTokenEntity> AddAsync(RefreshTokenEntity refreshToken)
        {
            if (refreshToken == null)
                throw new ArgumentNullException(nameof(refreshToken));

            const string query = @"
                INSERT INTO refresh_tokens (token, user_id, expires_at, created_at, is_revoked)
                VALUES (@Token, @UserId, @ExpiresAt, @CreatedAt, @IsRevoked)";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", refreshToken.Token },
                { "@UserId", refreshToken.UserId },
                { "@ExpiresAt", refreshToken.ExpiresAt },
                { "@CreatedAt", refreshToken.CreatedAt },
                { "@IsRevoked", refreshToken.IsRevoked }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            var rowsAffected = await _dbConnectionService.ExecuteNonQueryAsync(query, parameters, connection);

            if (rowsAffected > 0)
                return refreshToken;

            throw new InvalidOperationException("Failed to add refresh token.");
        }

        public async Task<RefreshTokenEntity?> GetByTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            const string query = @"
                SELECT token, user_id, expires_at, created_at, is_revoked 
                FROM refresh_tokens
                WHERE token = @Token AND is_revoked = 0";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", token }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            using var reader = await _dbConnectionService.ExecuteReaderAsync(query, parameters, connection);

            if (await reader.ReadAsync())
            {
                return new RefreshTokenEntity
                {
                    Token = reader.GetString("token"),
                    UserId = reader.GetGuid("user_id"),
                    ExpiresAt = reader.GetDateTime("expires_at"),
                    CreatedAt = reader.GetDateTime("created_at"),
                    IsRevoked = reader.GetBoolean("is_revoked")
                };
            }

            return null;
        }

        public async Task<bool> InvalidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            const string query = @"
                UPDATE refresh_tokens
                SET is_revoked = 1, 
                    revoked_at = @RevokedAt
                WHERE token = @Token";

            var parameters = new Dictionary<string, object>
            {
                { "@RevokedAt", DateTime.UtcNow },
                { "@Token", token }
            };

            using var connection = await _dbConnectionService.CreateConnectionAsync();
            var rowsAffected = await _dbConnectionService.ExecuteNonQueryAsync(query, parameters, connection);

            return rowsAffected > 0;
        }
    }

}
