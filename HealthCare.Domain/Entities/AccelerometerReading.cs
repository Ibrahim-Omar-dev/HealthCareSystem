using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthCare.Domain.Entities
{
    public class AccelerometerReading
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DeviceId { get; set; }

        public bool SensorOk { get; set; }

        public string ReadingsJson { get; set; } = "[]";

        public int ReadingsCount { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DeviceId")]
        public Device? Device { get; set; }
    }
}
