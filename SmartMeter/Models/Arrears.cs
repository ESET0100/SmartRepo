using SmartMeter.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMeter.Models
{
    [Table("Arrears")]
    public class Arrears
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ArrearId { get; set; }

        public long ConsumerId { get; set; }

        [ForeignKey("ConsumerId")]
        public virtual Consumer Consumer { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string ArrearType { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string PaidStatus { get; set; } = "Pending";

        public long BillId { get; set; }

        [ForeignKey("BillId")]
        public virtual Billing Billing { get; set; } = null!;

        [Required]
        [Column(TypeName = "numeric(18,2)")]
        public decimal ArrearAmount { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}