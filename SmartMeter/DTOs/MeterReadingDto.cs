namespace SmartMeter.DTOs
{
    public class MeterReadingDto
    {
        public long? ReadingId { get; set; }  // Added
        public DateOnly ReadingDate { get; set; }
        public decimal EnergyConsumed { get; set; }
        public string MeterSerialNo { get; set; } = string.Empty;
        public decimal? Current { get; set; }
        public decimal? Voltage { get; set; }
    }
}