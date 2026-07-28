using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime DateSent { get; set; } = DateTime.Now;
    }
}