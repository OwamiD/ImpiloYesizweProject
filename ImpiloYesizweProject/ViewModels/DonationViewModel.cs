using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.ViewModels
{
    public class DonationViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string DonorName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000000, ErrorMessage = "Please enter a valid donation amount.")]
        [Display(Name = "Donation Amount (R)")]
        public decimal Amount { get; set; }
    }
}