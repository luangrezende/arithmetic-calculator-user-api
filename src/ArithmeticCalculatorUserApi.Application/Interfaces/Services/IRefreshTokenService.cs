using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Application.Interfaces.Services
{
    public interface IRefreshTokenService
    {
        Task<string> AddAsync(Guid userId);

        Task<RefreshTokenDTO?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
