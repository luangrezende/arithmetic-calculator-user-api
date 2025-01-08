using ArithmeticCalculatorUserApi.Domain.Entities;

namespace ArithmeticCalculatorUserApi.Application.Interfaces.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokenEntity> AddAsync(RefreshTokenEntity refreshToken);

        Task<RefreshTokenEntity?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
