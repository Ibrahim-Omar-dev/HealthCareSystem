

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace HealthCare.Domain.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        [Key]
        public Guid UserId { get; set; }=Guid.NewGuid();
       
    }
}
