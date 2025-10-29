namespace SmartMeter.DTOs
{
    public class AddressDto
    {
        public long? AddressId { get; set; }  // Added
        public string HouseNumber { get; set; } = string.Empty;
        public string Locality { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public long ConsumerId { get; set; }
    }
}