using HealthCare.Application.Dto;
using HealthCare.Domain.Entities;

namespace HealthCare.Application.Interfaces
{
    public interface IMeasurementService
    {
        Task<IEnumerable<SensorMeasurement>> GetAllDataAsync();

        Task<IEnumerable<SensorMeasurement>> GetMyDataAsync(Guid userId);

        Task<IEnumerable<SensorMeasurement>?> GetUserDataAsync(Guid requesterId, Guid targetUserId);

        Task<(bool Success, string Message)> AddDataAsync(SensorMeasurementDto dto);

        Task<SensorMeasurement?> GetLastRecordAsync(Guid userId);

        Task<bool> IsFollowingAsync(Guid requesterId, Guid targetUserId);

        Task<SensorMeasurement> GetDataInLast6HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast12HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast24HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast3DaysAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast7DaysAsync(Guid userId);
    }
}