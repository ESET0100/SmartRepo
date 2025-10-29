using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartMeter.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long UserId { get; set; }

        [Required]
        [Column(TypeName = "varchar(100)")]
        public string Username { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        [Column(TypeName = "varchar(150)")]
        public string DisplayName { get; set; } = string.Empty;

        [Column(TypeName = "varchar(200)")]
        public string? Email { get; set; }

        [Column(TypeName = "varchar(30)")]
        public string? Phone { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime? LastLoginUtc { get; set; }

        public bool IsActive { get; set; } = true;

        [Column(TypeName = "timestamp with time zone")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}