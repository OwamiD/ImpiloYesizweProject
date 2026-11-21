using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter a username.")]
        [StringLength(100)]
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Please enter a password.")]
        [MinLength(12, ErrorMessage = "Password must be at least 12 characters.")]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required(ErrorMessage = "Please enter the administrator's full name.")]
        [StringLength(150)]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "Please enter an email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(254)]
        public string Email { get; set; } = "";
    }
}