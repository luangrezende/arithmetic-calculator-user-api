using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddAsync(RefreshToken refreshToken);

        Task<RefreshToken?> GetByTokenAsync(string token);

        Task InvalidateTokenAsync(string token);
    }
}
