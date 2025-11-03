using System.ComponentModel.DataAnnotations;

namespace SmartMeter.DTOs
{
    public class CreateConsumerDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Initialize with empty string

        [Required]
        public int OrgUnitId { get; set; }

        [Required]
        public int TariffId { get; set; }

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";
    }
}