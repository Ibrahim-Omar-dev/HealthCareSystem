using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.Entities
{
    public class Alert
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }

        public AlertType Type { get; set; }        
        public AlertCategory Category { get; set; } 

        public string Title { get; set; }
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}