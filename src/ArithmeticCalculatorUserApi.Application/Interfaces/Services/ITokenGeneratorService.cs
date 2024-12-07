using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Application.Interfaces.Services
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(UserDTO user);
    }
}
