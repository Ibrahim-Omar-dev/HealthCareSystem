

using HealthCare.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HealthCare.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        [Key]
        public Guid UserId { get; set; } = Guid.NewGuid();
        public Gender? Gender { get; set; }
        public string DisplayName { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public BloodType? BloodType { get; set; }
        //public string? AvatarUrl { get; set; }
    }
}
