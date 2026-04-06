using HealthCare.Application.Dto;
using HealthCare.Domain.Entities;
using HealthCare.Infreastructure.Data;
using HealthCare.Infreastructure.Services.Interface.IMeasurement;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infrastructure.Services.Implemention.Measurement
{
    public class MeasurementService : IMeasurementService
    {
        private readonly AppDbContext _context;

        public MeasurementService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SensorMeasurement>> GetAllDataAsync()
        {
            return await _context.Measurements.ToListAsync();
        }

        public async Task<IEnumerable<SensorMeasurement>> GetDataByUserIdAsync(Guid userId)
        {
            return await _context.Measurements
                .Where(m => m.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorMeasurement>> GetMyDataAsync(Guid userId)
        {
            return await _context.Measurements
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.RecordedAt)
                .ToListAsync();
        }

        public async Task<SensorMeasurement> AddDataAsync(SensorMeasurementDto dto, Guid? userId)
        {
            var measurement = new SensorMeasurement
            {
                device_id = Guid.NewGuid(),
                UserId = userId,
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

            return measurement;
        }

        public Task<SensorMeasurement?> GetLatestInLast6HoursAsync(Guid userId)
    => GetOldestInRangeAsync(userId, 6);

        public Task<SensorMeasurement?> GetLatestInLast12HoursAsync(Guid userId)
            => GetOldestInRangeAsync(userId, 12);

        public Task<SensorMeasurement?> GetLatestInLast24HoursAsync(Guid userId)
            => GetOldestInRangeAsync(userId, 24);

        public Task<SensorMeasurement?> GetLatestInLast3DaysAsync(Guid userId)
            => GetOldestInRangeAsync(userId, 72);

        public async Task<SensorMeasurement?> GetLastRecordAsync(Guid userId)
        {
            return await _context.Measurements
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.RecordedAt)        // ← الأقدم
                .FirstOrDefaultAsync();
        }

        private async Task<SensorMeasurement?> GetOldestInRangeAsync(Guid userId, int hours)
        {
            var from = DateTime.UtcNow.AddHours(-hours);

            return await _context.Measurements
                .Where(m => m.UserId == userId && m.RecordedAt >= from)
                .OrderBy(m => m.RecordedAt)        // ← الأقدم في النطاق
                .FirstOrDefaultAsync();
        }
    }
}