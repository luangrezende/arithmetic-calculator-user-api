using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Models.DTO;

namespace ArithmeticCalculatorUserApi.Domain.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserAutheticateDTO?> AuthenticateAsync(string username, string password);

        Task<bool> UserExistsAsync(string username);

        Task<bool> CreateUserAsync(string username, string password, string name);

        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
