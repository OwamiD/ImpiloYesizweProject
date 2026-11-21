using System.ComponentModel.DataAnnotations;

namespace ImpiloYesizweProject.ViewModels
{
    public class EmailVerificationViewModel
    {
        [Required(ErrorMessage = "Please enter your verification code.")]
        [RegularExpression(
            @"^\d{6}$",
            ErrorMessage = "Enter a valid six-digit code.")]
        public string Code { get; set; } = string.Empty;
    }
}