using System.ComponentModel.DataAnnotations;

namespace HealthCare.Application.Dto
{
    public class ResetPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = null!;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Otp { get; set; } = null!;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = null!;

        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; } = null!;
    }
}
