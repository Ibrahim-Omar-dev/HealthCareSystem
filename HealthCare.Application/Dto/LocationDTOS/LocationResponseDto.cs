using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCare.Application.Dto.LocationDTOS
{
    public class LocationResponseDto
    {
        public Guid Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime RecordedAt { get; set; }

        public string MapsUrl =>
            $"https://www.google.com/maps?q={Latitude},{Longitude}";
    }
}
