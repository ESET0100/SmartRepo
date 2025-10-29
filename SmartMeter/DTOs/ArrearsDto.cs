namespace SmartMeter.DTOs
{
    public class ArrearsDto
    {
        public long? ArrearId { get; set; }  // Added
        public long ConsumerId { get; set; }
        public string ArrearType { get; set; } = string.Empty;
        public string PaidStatus { get; set; } = "Pending";
        public long BillId { get; set; }
        public decimal ArrearAmount { get; set; }
    }
}