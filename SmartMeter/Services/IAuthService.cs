using SmartMeter.DTOs;
using SmartMeter.Models;

namespace SmartMeter.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<User?> RegisterAsync(User user, string password);
        Task<bool> UserExistsAsync(string username);

        Task<(bool Success, string Error)> ChangePasswordAsync(long userId, ChangePasswordDto request);
    }
}