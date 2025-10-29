namespace SmartMeter.DTOs
{
    public class TodRuleDto
    {
        public int? TodRuleId { get; set; }  // Added - nullable for POST
        public int TariffId { get; set; }
        public string Name { get; set; } = string.Empty;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal RatePerKwh { get; set; }
    }
}