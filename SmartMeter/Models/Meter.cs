using SmartMeter.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMeter.Models
{
    [Table("Meter")]
    public class Meter
    {
        [Key]
        [Column(TypeName = "varchar(50)")]
        public string MeterSerialNo { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(45)")]
        public string IpAddress { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string ICCID { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(30)")]
        public string IMSI { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Manufacturer { get; set; } = string.Empty;

        [Column(TypeName = "varchar(50)")]
        public string? Firmware { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Category { get; set; } = string.Empty;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime InstallTsUtc { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Status { get; set; } = "Active";

        public long ConsumerId { get; set; }

        [ForeignKey("ConsumerId")]
        public virtual Consumer Consumer { get; set; } = null!;

        public virtual ICollection<MeterReading> MeterReadings { get; set; } = new List<MeterReading>();
        public virtual ICollection<Billing> Billings { get; set; } = new List<Billing>();
    }
}