namespace SmartMeter.DTOs
{
    public class UserDto
    {
        public long? UserId { get; set; }  // Made nullable
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public DateTime? LastLoginUtc { get; set; }
        public bool IsActive { get; set; }
    }
}