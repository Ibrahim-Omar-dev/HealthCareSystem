using HealthCare.Application.Dto;
using HealthCare.Domain.Entities;

namespace HealthCare.Application.Interfaces
{
    public interface IMeasurementService
    {
        // ── Get All Data ──────────────────────────────────────────────────────
        Task<IEnumerable<SensorMeasurement>> GetAllDataAsync();

        // ── Get My Data ───────────────────────────────────────────────────────
        Task<IEnumerable<SensorMeasurement>> GetMyDataAsync(Guid userId);

        // ── Get Another User's Data (follow required) ─────────────────────────
        Task<IEnumerable<SensorMeasurement>?> GetUserDataAsync(Guid requesterId, Guid targetUserId);

        // ── Add Data (ESP32) ──────────────────────────────────────────────────
        Task<(bool Success, string Message)> AddDataAsync(SensorMeasurementDto dto);

        // ── Last Record ───────────────────────────────────────────────────────
        Task<SensorMeasurement?> GetLastRecordAsync(Guid userId);

        Task<bool> IsFollowingAsync(Guid requesterId, Guid targetUserId);

        Task<SensorMeasurement> GetDataInLast6HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast12HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast24HoursAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast3DaysAsync(Guid userId);
        Task<SensorMeasurement> GetDataInLast7DaysAsync(Guid userId);
    }
}