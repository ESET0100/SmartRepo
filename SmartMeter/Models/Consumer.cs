using SmartMeter.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Net;

namespace SmartMeter.Models
{
    [Table("Consumer")]
    public class Consumer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ConsumerId { get; set; }

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "varchar(30)")]
        public string? Phone { get; set; }

        [Column(TypeName = "varchar(200)")]
        public string? Email { get; set; }

        public int OrgUnitId { get; set; }

        [ForeignKey("OrgUnitId")]
        public virtual OrgUnit OrgUnit { get; set; } = null!;

        public int TariffId { get; set; }

        [ForeignKey("TariffId")]
        public virtual Tariff Tariff { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "Active";

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string CreatedBy { get; set; } = "system";

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? UpdatedAt { get; set; }

        [Column(TypeName = "varchar(100)")]
        public string? UpdatedBy { get; set; }

        public bool Deleted { get; set; } = false;

        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual ICollection<Meter> Meters { get; set; } = new List<Meter>();
        public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
    }
}