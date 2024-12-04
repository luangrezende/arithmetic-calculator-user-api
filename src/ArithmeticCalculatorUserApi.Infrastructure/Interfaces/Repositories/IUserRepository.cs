using ArithmeticCalculatorUserApi.Infrastructure.Models;

namespace ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<UserEntity?> GetUserByUsernameAsync(string username);

        Task<bool> CreateUserAsync(string username, string password, string name);

        Task<UserEntity?> GetUserByIdAsync(Guid userId);
    }
}

