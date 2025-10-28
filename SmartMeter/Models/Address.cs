using SmartMeter.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMeter.Models
{
    [Table("Address")]
    public class Address
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AddressId { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string HouseNumber { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Locality { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string City { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string State { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "varchar(10)")]
        public string Pincode { get; set; } = string.Empty;

        public long ConsumerId { get; set; }

        [ForeignKey("ConsumerId")]
        public virtual Consumer Consumer { get; set; } = null!;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        //shiavsuh
    }
}