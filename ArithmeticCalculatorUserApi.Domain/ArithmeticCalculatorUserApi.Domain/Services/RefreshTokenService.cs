using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Models.DTO;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;

namespace ArithmeticCalculatorUserApi.Domain.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public RefreshTokenService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<string> AddAsync(Guid userId)
        {
            var newRefreshToken = new RefreshToken
            {
                UserId = userId
            };

            await _refreshTokenRepository.AddAsync(newRefreshToken);

            return newRefreshToken.Token;
        }

        public async Task<RefreshTokenDTO?> GetByTokenAsync(string token)
        {
            var result = await _refreshTokenRepository.GetByTokenAsync(token);

            return result == null ? null : new RefreshTokenDTO
            {
                ExpiresAt = result.ExpiresAt,
                Token = result.Token,
                CreatedAt = result.CreatedAt,
                IsRevoked = result.IsRevoked,
                IsUsed = result.IsUsed,
                RevokedAt = result.RevokedAt,
                UserId = result.UserId
            };
        }

        public async Task InvalidateTokenAsync(string token)
        {
            await _refreshTokenRepository.InvalidateTokenAsync(token);
        }
    }
}
