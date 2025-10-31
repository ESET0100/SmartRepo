namespace SmartMeter.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public string UserType { get; set; } = "User"; // ADD THIS PROPERTY
        public DateTime ExpiresAt { get; set; }
    }
}