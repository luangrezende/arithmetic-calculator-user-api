using ArithmeticCalculatorUserApi.Domain.Models.DTO;

namespace ArithmeticCalculatorUserApi.Domain.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> AddAsync(Guid userId);

        Task<RefreshTokenDTO?> GetByTokenAsync(string token);

        Task<bool> InvalidateTokenAsync(string token);
    }
}
