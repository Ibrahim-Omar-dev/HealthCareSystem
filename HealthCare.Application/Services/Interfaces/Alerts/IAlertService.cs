using HealthCare.Application.Dto;
using HealthCare.Domain.Entities;

namespace HealthCare.Application.Interfaces
{
    public interface IAlertService
    {
        Task<IEnumerable<AlertDto>> GetMyAlertsAsync(Guid userId);
        Task<IEnumerable<AlertDto>> GetUnreadAlertsAsync(Guid userId);
        Task<IEnumerable<AlertDto>> GetCriticalAlertsAsync(Guid userId);
        Task MarkAsReadAsync(Guid alertId, Guid userId);
        Task MarkAllAsReadAsync(Guid userId);
        Task GenerateAlertsFromMeasurementAsync(SensorMeasurement m, Guid userId);
    }
}