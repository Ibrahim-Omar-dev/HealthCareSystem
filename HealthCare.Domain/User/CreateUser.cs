using HealthCare.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace HealthCare.Domain.User
{
    public class CreateUser : BaseModel
    {
        public required string UserName { get; set; }
        public required string ConfirmPassword { get; set; }
        public Gender? Gender { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }
        public BloodType? BloodType { get; set; }
        public string? DeviceCode { get; set; }


    }
}
