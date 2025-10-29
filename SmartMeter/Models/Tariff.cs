using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("Tariff")]
    public class Tariff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TariffId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal BaseRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TaxRate { get; set; } = 0;

        public virtual ICollection<TodRule> TodRules { get; set; } = new List<TodRule>();
        public virtual ICollection<TariffSlab> TariffSlabs { get; set; } = new List<TariffSlab>();
        public virtual ICollection<Consumer> Consumers { get; set; } = new List<Consumer>();
    }
}