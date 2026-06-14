using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.Enums;

namespace HealthCare.Domain.Entities
{
    public class Medicine
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            public Guid UserId { get; set; }

            public string Name { get; set; } = string.Empty;
            public string Dosage { get; set; } = string.Empty;

            public MedicineFrequency Frequency { get; set; }

            // Reminder times — بيتخزن كـ JSON string  e.g. ["08:00","14:00","20:00"]
            public string ReminderTimesJson { get; set; } = "[]";

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }

            public string? Notes { get; set; }

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [ForeignKey("UserId")]
            public AppUser? User { get; set; }
        }
    }