using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("TariffSlab")]
    public class TariffSlab
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TariffSlabId { get; set; }

        [Required]
        public int TariffId { get; set; }

        [ForeignKey("TariffId")]
        [JsonIgnore]
        public virtual Tariff Tariff { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal FromKwh { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal ToKwh { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal RatePerKwh { get; set; }

        public bool Deleted { get; set; } = false;
    }
}