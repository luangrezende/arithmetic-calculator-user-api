using ArithmeticCalculatorUserApi.Application.DTOs;
using ArithmeticCalculatorUserApi.Application.Interfaces.Repositories;
using ArithmeticCalculatorUserApi.Application.Interfaces.Services;

namespace ArithmeticCalculatorUserApi.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISecurityService _securityService;

        public UserService(
            IUserRepository userRepository,
            ISecurityService securityService)
        {
            _userRepository = userRepository;
            _securityService = securityService;
        }

        public async Task<UserDTO?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetUserByUsernameAsync(username);

            if (user == null)
                return null;

            if (!_securityService.VerifyPassword(password, user.Password!))
                return null;

            return new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Status = user.Status,
                Name = user.Name,
            };
        }

        public async Task<bool> CreateUserAsync(string username, string password, string name)
        {
            return await _userRepository.CreateUserAsync(username, password, name);
        }

        public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
        {
            var result = await _userRepository.GetUserByIdAsync(userId);

            return result == null ? null : new UserDTO
            {
                Id = result!.Id,
                Username = result.Username,
                Status = result.Status,
                Name = result.Name,
            };
        }

        public async Task<UserDTO?> GetUserByUsernameAsync(string username)
        {
            var result = await _userRepository.GetUserByUsernameAsync(username);

            return result == null ? null : new UserDTO
            {
                Id = result!.Id,
                Username = result.Username,
                Status = result.Status,
                Name = result.Name,
            };
        }
    }
}
