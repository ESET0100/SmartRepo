using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("TodRule")]
    public class TodRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TodRuleId { get; set; }

        [Required]
        public int TariffId { get; set; }

        [ForeignKey("TariffId")]
        [JsonIgnore]
        public virtual Tariff Tariff { get; set; } = null!;

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "time(0)")]
        public TimeOnly StartTime { get; set; }

        [Required]
        [Column(TypeName = "time(0)")]
        public TimeOnly EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,6)")]
        public decimal RatePerKwh { get; set; }

        public bool Deleted { get; set; } = false;
    }
}