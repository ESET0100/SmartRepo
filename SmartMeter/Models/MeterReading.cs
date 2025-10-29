using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("MeterReading")]
    public class MeterReading
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ReadingId { get; set; }

        [Required]
        public DateOnly ReadingDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal EnergyConsumed { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string MeterSerialNo { get; set; } = string.Empty;

        [ForeignKey("MeterSerialNo")]
        [JsonIgnore]
        public virtual Meter Meter { get; set; } = null!;

        [Column(TypeName = "decimal(8,3)")]
        public decimal? Current { get; set; }

        [Column(TypeName = "decimal(8,3)")]
        public decimal? Voltage { get; set; }
    }
}