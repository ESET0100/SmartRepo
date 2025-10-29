namespace SmartMeter.DTOs
{
    public class BillingDto
    {
        public long? BillId { get; set; }  // Added
        public long ConsumerId { get; set; }
        public string MeterId { get; set; } = string.Empty;
        public DateOnly BillingPeriodStart { get; set; }
        public DateOnly BillingPeriodEnd { get; set; }
        public decimal TotalUnitsConsumed { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public DateOnly DueDate { get; set; }
        public string PaymentStatus { get; set; } = "Unpaid";
        public DateTime? PaidDate { get; set; }
    }
}