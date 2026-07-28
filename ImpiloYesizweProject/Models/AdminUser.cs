using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.Models
{
    public class AdminUser
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public string FullName { get; set; } = "";

        public string Email { get; set; } = "";
    }
}