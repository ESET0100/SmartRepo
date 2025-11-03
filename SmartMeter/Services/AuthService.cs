using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;
using SmartMeter.DTOs;
using SmartMeter.Helpers;
using SmartMeter.Models;
using System.IdentityModel.Tokens.Jwt;
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

        // USER AUTHENTICATION METHODS
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

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
                UserType = "User", // ADD THIS
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task<User?> RegisterAsync(User user, string password)
        {
            if (await UserExistsAsync(user.Username))
                return null;

            user.PasswordHash = HashPassword(password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        // CONSUMER AUTHENTICATION METHODS
        public async Task<ConsumerAuthResponseDto?> ConsumerLoginAsync(ConsumerLoginDto loginDto)
        {
            var consumer = await _context.Consumers
                .FirstOrDefaultAsync(c => c.Email == loginDto.Email && !c.Deleted && c.Status == "Active");

            if (consumer == null || !VerifyPassword(loginDto.Password, consumer.PasswordHash))
                return null;

            var token = _jwtHelper.GenerateToken(consumer);

            return new ConsumerAuthResponseDto
            {
                Token = token,
                Consumer = new ConsumerDto
                {
                    ConsumerId = consumer.ConsumerId,
                    Name = consumer.Name,
                    Phone = consumer.Phone,
                    Email = consumer.Email,
                    OrgUnitId = consumer.OrgUnitId,
                    TariffId = consumer.TariffId,
                    Status = consumer.Status
                },
                UserType = "Consumer",
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };
        }

        public async Task<Consumer?> CreateConsumerWithPasswordAsync(Consumer consumer, string password)
        {
            if (await ConsumerExistsAsync(consumer.Email!)) // FIX NULL REFERENCE
                return null;

            consumer.PasswordHash = HashPassword(password);
            consumer.CreatedAt = DateTime.UtcNow;
            consumer.CreatedBy = "system";

            _context.Consumers.Add(consumer);
            await _context.SaveChangesAsync();

            return consumer;
        }

        public async Task<bool> ConsumerExistsAsync(string email)
        {
            return await _context.Consumers.AnyAsync(c => c.Email == email && !c.Deleted);
        }

        public async Task<bool> UserExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        // PASSWORD METHODS
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

        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty");

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters long");

            return (true, string.Empty);
        }

        public async Task<(bool, string)> ChangePasswordAsync(long userId, ChangePasswordDto request)
        {
            if (request.NewPassword != request.ConfirmPassword)
            {
                return (false, "New password and confirmation do not match");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            var passwordValidationResult = ValidatePassword(request.NewPassword);
            if (!passwordValidationResult.IsValid)
            {
                return (false, passwordValidationResult.ErrorMessage);
            }

            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return (true, "Password changed successfully");
        }
    }
}