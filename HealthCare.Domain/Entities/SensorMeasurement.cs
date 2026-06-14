using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.Entities
{
    public class SensorMeasurement
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DeviceId { get; set; }

        public double bpm { get; set; }
        public double spo2 { get; set; }
        public double resp_rate { get; set; }
        public double temp { get; set; }

        public bool fall_detected { get; set; }
        public string? fall_type { get; set; }

        public double lat { get; set; }
        public double lng { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
    }
}