namespace ArithmeticCalculatorUserApi.Domain.Repositories
{
    public interface IUserRepository
    {
        (int userId, string username, string status)? Authenticate(string username, string password);
        bool UserExists(string username);
        bool CreateUser(string username, string password, string name); // Atualizado
    }
}

