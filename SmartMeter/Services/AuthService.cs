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
        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                return null;

            user.LastLoginUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);

            // ✅ DEBUG: Check the generated token
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // This will show in Swagger/API response
            Console.WriteLine("=== TOKEN GENERATED ===");
            foreach (var claim in jwtToken.Claims)
            {
                Console.WriteLine($"{claim.Type}: {claim.Value}");
            }

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
                ExpiresAt = DateTime.UtcNow.AddMinutes(60)
            };
        }
        //Lakshay code
        //public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        //{
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

        //    if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
        //        return null;

        //    // USE UTC
        //    user.LastLoginUtc = DateTime.UtcNow;
        //    await _context.SaveChangesAsync();

        //    var token = _jwtHelper.GenerateToken(user);

        //    return new AuthResponseDto
        //    {
        //        Token = token,
        //        User = new UserDto
        //        {
        //            UserId = user.UserId,
        //            Username = user.Username,
        //            DisplayName = user.DisplayName,
        //            Email = user.Email,
        //            Phone = user.Phone,
        //            LastLoginUtc = user.LastLoginUtc,
        //            IsActive = user.IsActive
        //        },
        //        ExpiresAt = DateTime.UtcNow.AddMinutes(60) // USE UTC
        //    };
        //}

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


        private (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty");

            if (password.Length < 6)
                return (false, "Password must be at least 6 characters long");

            // Add more validation rules as needed
            return (true, string.Empty);
        }
        public async Task<(bool, string)> ChangePasswordAsync(long userId, ChangePasswordDto request)
        {
            // Validate new password and confirmation
            if (request.NewPassword != request.ConfirmPassword)
            {
                return (false, "New password and confirmation do not match");
            }

            // Find user
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return (false, "User not found");
            }

            // ✅ FIX: Use VerifyPassword instead of direct comparison
            if (!VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return (false, "Current password is incorrect");
            }

            // Validate new password strength
            var passwordValidationResult = ValidatePassword(request.NewPassword);
            if (!passwordValidationResult.IsValid)
            {
                return (false, passwordValidationResult.ErrorMessage);
            }

            // Update password
            user.PasswordHash = HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return (true, "Password changed successfully");
        }
    }
}