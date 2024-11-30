using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> AuthenticateAsync(string username, string password);

        Task<bool> UserExistsAsync(string username);

        Task<bool> UserIsActiveAsync(string username);

        Task<bool> CreateUserAsync(string username, string password, string name);

        Task<User?> GetUserByIdAsync(Guid userId);
    }
}

