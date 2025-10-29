namespace SmartMeter.DTOs
{
    public class MeterDto
    {
        public string? MeterSerialNo { get; set; }  // Added - nullable for POST
        public string IpAddress { get; set; } = string.Empty;
        public string ICCID { get; set; } = string.Empty;
        public string IMSI { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? Firmware { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime InstallTsUtc { get; set; }
        public string Status { get; set; } = "Active";
        public long ConsumerId { get; set; }
    }
}