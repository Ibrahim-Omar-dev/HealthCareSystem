using HealthCare.Application.Dto;
using HealthCare.Domain.Entities;

namespace HealthCare.Infreastructure.Services.Interface.IMeasurement
{
    public interface IMeasurementService
    {
        Task<IEnumerable<SensorMeasurement>> GetAllDataAsync();
        Task<IEnumerable<SensorMeasurement>> GetDataByUserIdAsync(Guid userId);
        Task<IEnumerable<SensorMeasurement>> GetMyDataAsync(Guid userId);
        Task<SensorMeasurement> AddDataAsync(SensorMeasurementDto dto, Guid? userId);

        Task<SensorMeasurement?> GetLatestInLast6HoursAsync(Guid userId);
        Task<SensorMeasurement?> GetLatestInLast12HoursAsync(Guid userId);
        Task<SensorMeasurement?> GetLatestInLast24HoursAsync(Guid userId);
        Task<SensorMeasurement?> GetLatestInLast3DaysAsync(Guid userId);
        Task<SensorMeasurement?> GetLastRecordAsync(Guid userId);
    }
}
