using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Dto
{
        public class SensorMeasurementDto
        {
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
        }
}
