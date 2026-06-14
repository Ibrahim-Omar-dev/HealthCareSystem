using HealthCare.Application.Dto;
using HealthCare.Application.Interfaces;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Services.Implementation.Measurement
{
    public class MeasurementService : IMeasurementService
    {
        private readonly AppDbContext _context;
        private readonly IAlertService _alertService;

        public MeasurementService(AppDbContext context, IAlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        // ── Private Helper ────────────────────────────────────────────────────

        // ✅ FIX: Device is a separate table — join through Devices not Users
        private async Task<Guid?> GetDeviceIdByUserIdAsync(Guid userId)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId);

            return device?.Id;
        }

        // ── IsFollowing ───────────────────────────────────────────────────────

        public async Task<bool> IsFollowingAsync(Guid requesterId, Guid targetUserId)
        {
            return await _context.FollowRequests
                .AnyAsync(f =>
                    f.SenderId == requesterId &&
                    f.ReceiverId == targetUserId &&
                    f.Status == FollowRequestStatus.Accepted);
        }

        // ── Get All Data ──────────────────────────────────────────────────────

        public async Task<IEnumerable<SensorMeasurement>> GetAllDataAsync()
        {
            return await _context.Measurements
                .Include(m => m.Device)
                .ToListAsync();
        }

        // ── Get My Data ───────────────────────────────────────────────────────

        public async Task<IEnumerable<SensorMeasurement>> GetMyDataAsync(Guid userId)
        {
            var deviceId = await GetDeviceIdByUserIdAsync(userId);

            if (deviceId is null)
                return Enumerable.Empty<SensorMeasurement>();

            return await _context.Measurements
                .Where(m => m.DeviceId == deviceId)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();
        }

        // ── Get Another User's Data (follow check) ────────────────────────────

        public async Task<IEnumerable<SensorMeasurement>?> GetUserDataAsync(
            Guid requesterId, Guid targetUserId)
        {
            var isFollowing = await IsFollowingAsync(requesterId, targetUserId);

            if (!isFollowing)
                return null; // Controller returns 403

            var deviceId = await GetDeviceIdByUserIdAsync(targetUserId);

            if (deviceId is null)
                return Enumerable.Empty<SensorMeasurement>();

            return await _context.Measurements
                .Where(m => m.DeviceId == deviceId)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();
        }

        // ── Add Data ──────────────────────────────────────────────────────────

        public async Task<(bool Success, string Message)> AddDataAsync(SensorMeasurementDto dto)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceCode == dto.DeviceCode && d.IsActive);

            if (device is null)
                return (false, "Device not found or inactive.");

            var measurement = new SensorMeasurement
            {
                DeviceId = device.Id,
                bpm = dto.bpm,
                spo2 = dto.spo2,
                resp_rate = dto.resp_rate,
                temp = dto.temp,
                fall_detected = dto.fall_detected,
                fall_type = dto.fall_type,
                lat = dto.lat,
                lng = dto.lng,
                RecordedAt = DateTime.UtcNow
            };

            _context.Measurements.Add(measurement);
            await _context.SaveChangesAsync();

            // ✅ FIX: get owner from Devices table not Users
            await _alertService.GenerateAlertsFromMeasurementAsync(measurement, device.UserId);

            return (true, "Measurement added successfully.");
        }

        // ── Last Record ───────────────────────────────────────────────────────

        public async Task<SensorMeasurement?> GetLastRecordAsync(Guid userId)
        {
            var deviceId = await GetDeviceIdByUserIdAsync(userId);
            if (deviceId is null) return null;

            return await _context.Measurements
                .Where(m => m.DeviceId == deviceId)
                .OrderByDescending(m => m.RecordedAt) 
                .FirstOrDefaultAsync();
        }

        // ── Time-Range Queries ────────────────────────────────────────────────

        // ✅ FIX: return type is SensorMeasurement? not SensorMeasurement
        public Task<SensorMeasurement?> GetDataInLast6HoursAsync(Guid userId)
            => GetDataInRangeAsync(userId, hours: 6);

        public Task<SensorMeasurement?> GetDataInLast12HoursAsync(Guid userId)
            => GetDataInRangeAsync(userId, hours: 12);

        public Task<SensorMeasurement?> GetDataInLast24HoursAsync(Guid userId)
            => GetDataInRangeAsync(userId, hours: 24);

        public Task<SensorMeasurement?> GetDataInLast3DaysAsync(Guid userId)
            => GetDataInRangeAsync(userId, hours: 72);

        public Task<SensorMeasurement?> GetDataInLast7DaysAsync(Guid userId)
            => GetDataInRangeAsync(userId, hours: 168);

        private async Task<SensorMeasurement?> GetDataInRangeAsync(Guid userId, int hours)
        {
            var deviceId = await GetDeviceIdByUserIdAsync(userId);
            if (deviceId is null) return null;

            var from = DateTime.UtcNow.AddHours(-hours);

            return await _context.Measurements
                .Where(m => m.DeviceId == deviceId && m.RecordedAt >= from)
                .OrderBy(m => m.RecordedAt)         
                .FirstOrDefaultAsync();
        }
    }
}