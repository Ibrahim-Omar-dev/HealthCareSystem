namespace HealthCare.Application.Dto.Accelerometer
{
    public class AccelerometerResponseDto
    {
        public Guid Id { get; set; }
        public bool SensorOk { get; set; }
        public int ReadingsCount { get; set; }
        public DateTime RecordedAt { get; set; }

        public List<List<double>> Readings { get; set; } = new();

        public AccelerometerStatsDto Stats { get; set; } = new();
    }
}
