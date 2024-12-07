
using ArithmeticCalculatorUserApi.Application.DTOs;

namespace ArithmeticCalculatorUserApi.Infrastructure.Interfaces.Services
{
    public interface ITokenGeneratorService
    {
        string GenerateToken(UserDTO user);
    }
}
