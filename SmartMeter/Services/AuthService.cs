using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Helpers;
using SmartMeter.Models;
using System.Text;

namespace SmartMeter.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthService(ApplicationDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            // USE UTC
            user.LastLoginUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = new UserDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    Phone = user.Phone,
                    LastLoginUtc = user.LastLoginUtc,
                    IsActive = user.IsActive
                },
                ExpiresAt = DateTime.UtcNow.AddMinutes(60) // USE UTC
            };
        }

        // Remove any CreatedAt assignment in RegisterAsync
        public async Task<User?> RegisterAsync(User user, string password)
        {
            if (await UserExistsAsync(user.Username))
                return null;

            user.PasswordHash = HashPassword(password);
            // NO CreatedAt assignment - it's auto-set in model

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        private byte[] HashPassword(string password)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            return Encoding.UTF8.GetBytes(hashed);
        }

        private bool VerifyPassword(string password, byte[] storedHash)
        {
            var hashString = Encoding.UTF8.GetString(storedHash);
            return BCrypt.Net.BCrypt.Verify(password, hashString);
        }
    }
}