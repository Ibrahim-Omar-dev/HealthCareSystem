using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Dto.Accelerometer
{
    public class AddAccelerometerDto
    {
        public string DeviceCode { get; set; } = string.Empty;
        public bool SensorOk { get; set; }

        public List<List<double>> Readings { get; set; } = new();
    }
}
