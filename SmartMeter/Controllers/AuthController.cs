using Microsoft.AspNetCore.Mvc;
using SmartMeter.DTOs;
using SmartMeter.Services;
using Microsoft.EntityFrameworkCore;
using SmartMeter.Data;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context; // ADD THIS
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (result == null)
                return Unauthorized("Invalid username or password");

            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult> Register([FromBody] LoginDto registerDto)
        {
            var user = new Models.User
            {
                Username = registerDto.Username,
                DisplayName = registerDto.Username,
                IsActive = true
            };

            var result = await _authService.RegisterAsync(user, registerDto.Password);

            if (result == null)
                return BadRequest("Username already exists");

            return Ok(new { message = "User registered successfully" });
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new UserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    DisplayName = u.DisplayName,
                    Email = u.Email,
                    Phone = u.Phone,
                    LastLoginUtc = u.LastLoginUtc,
                    IsActive = u.IsActive
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(long id, UserDto userDto)
        {
            if (userDto.UserId.HasValue && id != userDto.UserId.Value)
                return BadRequest("ID mismatch");

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            // Update fields
            existingUser.Username = userDto.Username;
            existingUser.DisplayName = userDto.DisplayName;
            existingUser.Email = userDto.Email;
            existingUser.Phone = userDto.Phone;
            existingUser.IsActive = userDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ADD THIS METHOD:
        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.UserId == id && e.IsActive);
        }
    }
}