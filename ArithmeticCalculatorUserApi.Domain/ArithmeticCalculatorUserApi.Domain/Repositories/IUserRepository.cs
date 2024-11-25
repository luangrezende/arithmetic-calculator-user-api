using ArithmeticCalculatorUserApi.Domain.Models;

namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IUserRepository
    {
        AuthenticateUser? Authenticate(string username, string password);

        bool UserExists(string username);

        bool CreateUser(string username, string password, string name);

        AuthenticateUser? GetUserById(Guid userId);
    }
}

