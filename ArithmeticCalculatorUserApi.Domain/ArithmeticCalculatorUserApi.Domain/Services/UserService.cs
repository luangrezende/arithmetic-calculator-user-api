using ArithmeticCalculatorUserApi.Domain.Models;
using ArithmeticCalculatorUserApi.Domain.Models.DTO;
using ArithmeticCalculatorUserApi.Domain.Repositories;
using ArithmeticCalculatorUserApi.Domain.Services.Interfaces;

namespace ArithmeticCalculatorUserApi.Domain.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserAutheticateDTO?> AuthenticateAsync(string username, string password)
        {
            var result = await _userRepository.AuthenticateAsync(username, password);

            return new UserAutheticateDTO 
            {
                Id = result!.Id,
                Username = result.Username,
                Status = result.Status,
                Name = result.Name,
                Accounts = result.Accounts
            };
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            return await _userRepository.CreateUserAsync(username, password, name);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _userRepository.UserExistsAsync(username);
        }
    }
}
