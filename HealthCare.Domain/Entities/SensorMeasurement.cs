using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthCare.Domain.Entities
{
    public class SensorMeasurement
    {
        [Key]
        public Guid Id { get; set; } =Guid.NewGuid();
        public Guid UserId { get; set; }

        public decimal? Temperature { get; set; }
        public decimal? OxygenLevel { get; set; }

        public int? SystolicPressure { get; set; }
        public int? DiastolicPressure { get; set; }

        public int? HeartRate { get; set; }

        public decimal? Depth { get; set; }
        public decimal? Pressure { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public bool FallDetected { get; set; }

        public DateTime RecordedAt { get; set; }=DateTime.Now;
    }
}
