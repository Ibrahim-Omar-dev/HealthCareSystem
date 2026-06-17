using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Dto
{
    public class SensorMeasurementDto
    {
        public string DeviceCode { get; set; }

        public double bpm { get; set; }
        public double spo2 { get; set; }
        public double resp_rate { get; set; }
        public double temp { get; set; }

        public bool fall_detected { get; set; }
        public string? fall_type { get; set; }

        public double? lat { get; set; } = null;
        public double? lng { get; set; }=null;
    }
}
