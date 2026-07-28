using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.ViewModels
{
    public class ContactViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
    }
}