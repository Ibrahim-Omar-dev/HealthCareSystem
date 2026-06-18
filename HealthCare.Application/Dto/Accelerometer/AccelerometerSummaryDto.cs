namespace HealthCare.Application.Dto.Accelerometer
{
    public class AccelerometerSummaryDto
    {
        public Guid Id { get; set; }
        public bool SensorOk { get; set; }
        public int ReadingsCount { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
