using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HealthCare.Domain.Entities.Identity;

namespace HealthCare.Domain.Entities
{
    public class SensorMeasurement
    {
        [Key]
        public Guid device_id { get; set; }

        public Guid? UserId { get; set; }

        // Vitals
        public double bpm { get; set; }
        public double spo2 { get; set; }
        public double resp_rate { get; set; }
        public double temp { get; set; }

        // Fall detection
        public bool fall_detected { get; set; }
        public string? fall_type { get; set; }

        // Location
        public double lat { get; set; }
        public double lng { get; set; }

        public DateTime RecordedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public AppUser? User { get; set; }
    }
}