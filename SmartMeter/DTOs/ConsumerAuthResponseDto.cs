namespace SmartMeter.DTOs
{
    public class ConsumerAuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public ConsumerDto Consumer { get; set; } = null!;
        public string UserType { get; set; } = "Consumer";
        public DateTime ExpiresAt { get; set; }
    }
}