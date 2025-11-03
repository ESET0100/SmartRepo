using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartMeter.Models
{
    [Table("LoginLog")]
    public class LoginLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LogId { get; set; }

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string UserType { get; set; } = string.Empty; // "User" or "Consumer"

        public long? UserId { get; set; } // Nullable for failed attempts

        public long? ConsumerId { get; set; } // Nullable for failed attempts

        [Required]
        [Column(TypeName = "varchar(200)")]
        public string Identifier { get; set; } = string.Empty; // Username or Email

        [Required]
        [Column(TypeName = "varchar(20)")]
        public string AttemptResult { get; set; } = string.Empty; // "Success", "InvalidPassword", "UserNotFound", "Inactive", "Deleted"

        [Column(TypeName = "varchar(45)")]
        public string? IpAddress { get; set; }

        [Column(TypeName = "varchar(500)")]
        public string? UserAgent { get; set; }

        [Column(TypeName = "timestamp with time zone")]
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "text")]
        public string? AdditionalInfo { get; set; }
    }
}