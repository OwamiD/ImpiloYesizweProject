
using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class Donation
    {
        public int Id { get; set; }

        [Required]
        public string DonorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000)]
        public decimal Amount { get; set; }

        public DateTime DonationDate { get; set; } = DateTime.Now;

        // Donation record protection
        public bool IsVoided { get; set; } = false;

        // Audit information for voided donations
        public DateTime? VoidedAtUtc { get; set; }

        [MaxLength(100)]
        public string? VoidedBy { get; set; }

        [MaxLength(500)]
        public string? VoidReason { get; set; }
    }
}
