namespace SmartMeter.DTOs
{
    public class ConsumerDto
    {
        public long? ConsumerId { get; set; }  // Added
        public string Name { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int OrgUnitId { get; set; }
        public int TariffId { get; set; }
        public string Status { get; set; } = "Active";
    }
}