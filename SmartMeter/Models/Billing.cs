using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("Billing")]
    public class Billing
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long BillId { get; set; }

        [Required]
        public long ConsumerId { get; set; }

        [ForeignKey("ConsumerId")]
        [JsonIgnore]
        public virtual Consumer Consumer { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string MeterId { get; set; } = string.Empty;

        [ForeignKey("MeterId")]
        [JsonIgnore]
        public virtual Meter Meter { get; set; } = null!;

        [Required]
        public DateOnly BillingPeriodStart { get; set; }

        [Required]
        public DateOnly BillingPeriodEnd { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,6)")]
        public decimal TotalUnitsConsumed { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,4)")]
        public decimal BaseAmount { get; set; }

        [Required]
        [Column(TypeName = "numeric(18,4)")]
        public decimal TaxAmount { get; set; } = 0;

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "numeric(18,4)")]
        public decimal TotalAmount { get; private set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateOnly DueDate { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? PaidDate { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string PaymentStatus { get; set; } = "Unpaid";

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? DisconnectionDate { get; set; }

        [JsonIgnore]
        public virtual ICollection<Arrears> Arrears { get; set; } = new List<Arrears>();
    }
}