namespace ArithmeticCalculatorUserApi.Application.Interfaces.Services
{
    public interface ISecurityService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string hashedPassword);
    }
}
