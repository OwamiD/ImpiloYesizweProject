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

    }
}