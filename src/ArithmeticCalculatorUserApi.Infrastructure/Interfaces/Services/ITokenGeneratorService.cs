using ArithmeticCalculatorUserApi.Infrastructure.Models;

namespace ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(UserEntity user);
    }
}
