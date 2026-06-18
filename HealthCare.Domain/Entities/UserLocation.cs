using HealthCare.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.Entities
{
    public class UserLocation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}
