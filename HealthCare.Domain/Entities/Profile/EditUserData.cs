using HealthCare.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace HealthCare.Domain.Entities.Profile
{
    public class EditUserData
    {
        public string DisplayName { get; set; }
        [EmailAddress]
        public string Email{ get; set; }
        [Phone]
        public string? PhoneNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender  { get; set; }
        public BloodType? BloodType { get; set; }
    }
}
