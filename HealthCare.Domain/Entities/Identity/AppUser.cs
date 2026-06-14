using HealthCare.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<Guid>  
    {
        public Gender? Gender { get; set; }
        public string DisplayName { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public BloodType? BloodType { get; set; }
        public Guid? DeviceId { get; set; }


        public string? OtpCode { get; set; }
        public DateTime? OtpExpiry { get; set; }
    }
   
}