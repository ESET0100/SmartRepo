using Microsoft.AspNetCore.Mvc;
using SmartMeter.DTOs;
using SmartMeter.Services;

namespace SmartMeter.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
    }
}