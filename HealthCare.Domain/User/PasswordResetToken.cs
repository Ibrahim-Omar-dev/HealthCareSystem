using HealthCare.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.User
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; } = Guid.Empty;

        public DateTime Expiry { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = null!;
    }
}