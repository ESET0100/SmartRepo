using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMeter.Models
{
    [Table("OrgUnit")]
    public class OrgUnit
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrgUnitId { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Name { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public virtual OrgUnit? Parent { get; set; }

        public virtual ICollection<OrgUnit> Children { get; set; } = new List<OrgUnit>();
        public virtual ICollection<Consumer> Consumers { get; set; } = new List<Consumer>();
    }
}