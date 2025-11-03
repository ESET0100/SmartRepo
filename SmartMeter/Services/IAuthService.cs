using SmartMeter.DTOs;
using SmartMeter.Models;

namespace SmartMeter.Services
{
    public interface IAuthService
    {
        // User methods
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<User?> RegisterAsync(User user, string password);
        Task<bool> UserExistsAsync(string username);

        // Consumer methods
        Task<ConsumerAuthResponseDto?> ConsumerLoginAsync(ConsumerLoginDto loginDto);
        Task<Consumer?> CreateConsumerWithPasswordAsync(Consumer consumer, string password);
        Task<bool> ConsumerExistsAsync(string email);

        Task<(bool Success, string Error)> ChangePasswordAsync(long userId, ChangePasswordDto request);
    }
}