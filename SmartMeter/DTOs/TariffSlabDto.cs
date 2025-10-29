namespace SmartMeter.DTOs
{
    public class TariffSlabDto
    {
        public int? TariffSlabId { get; set; }  // Added
        public int TariffId { get; set; }
        public decimal FromKwh { get; set; }
        public decimal ToKwh { get; set; }
        public decimal RatePerKwh { get; set; }
    }
}