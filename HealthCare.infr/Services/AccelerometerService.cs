using HealthCare.Application.Dto.Accelerometer;
using HealthCare.Application.Services.Interfaces;
using HealthCare.Domain.Entities;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace HealthCare.Infreastructure.Services
{
    public class AccelerometerService : IAccelerometerService
    {
        private readonly AppDbContext _context;

        public AccelerometerService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<(bool Success, string Message)> AddReadingsAsync(AddAccelerometerDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceCode))
                return (false, "DeviceCode is required.");

            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.DeviceCode == dto.DeviceCode && d.IsActive);

            if (device is null)
                return (false, "Device not found or inactive.");

            if (dto.Readings is null || !dto.Readings.Any())
                return (false, "No readings provided.");

            if (dto.Readings.Any(r => r.Count != 3))
                return (false, "Each reading must contain exactly 3 values [X, Y, Z].");

            var reading = new AccelerometerReading
            {
                DeviceId = device.Id,
                SensorOk = dto.SensorOk,
                ReadingsJson = JsonSerializer.Serialize(dto.Readings),
                ReadingsCount = dto.Readings.Count,
                RecordedAt = DateTime.UtcNow
            };

            _context.AccelerometerReadings.Add(reading);
            await _context.SaveChangesAsync();

            return (true, $"Readings saved successfully. {dto.Readings.Count} data points recorded.");
        }


        public async Task<object?> GetMyReadingsAsync()
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync();

            if (device is null) return null;

            var reading = await _context.AccelerometerReadings
                .Where(r => r.DeviceId == device.Id)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            if (reading is null) return null;

            var readings = JsonSerializer.Deserialize<List<List<double>>>(reading.ReadingsJson)
                           ?? new List<List<double>>();

            return new
            {
                readings = readings
            };
        }


        public async Task<AccelerometerResponseDto?> GetReadingByIdAsync(Guid userId, Guid readingId)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (device is null) return null;

            var reading = await _context.AccelerometerReadings
                .FirstOrDefaultAsync(r => r.Id == readingId && r.DeviceId == device.Id);

            return reading is null ? null : MapToDto(reading);
        }


        public async Task<AccelerometerResponseDto?> GetLastReadingAsync(Guid userId)
        {
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (device is null) return null;

            var reading = await _context.AccelerometerReadings
                .Where(r => r.DeviceId == device.Id)
                .OrderByDescending(r => r.RecordedAt)
                .FirstOrDefaultAsync();

            return reading is null ? null : MapToDto(reading);
        }


        private static AccelerometerResponseDto MapToDto(AccelerometerReading r)
        {
            var readings = JsonSerializer.Deserialize<List<List<double>>>(r.ReadingsJson)
                           ?? new List<List<double>>();

            return new AccelerometerResponseDto
            {
                Id = r.Id,
                SensorOk = r.SensorOk,
                ReadingsCount = r.ReadingsCount,
                RecordedAt = r.RecordedAt,
                Readings = readings,
                Stats = CalculateStats(readings)
            };
        }

        private static AccelerometerStatsDto CalculateStats(List<List<double>> readings)
        {
            if (!readings.Any())
                return new AccelerometerStatsDto();

            var xValues = readings.Select(r => r[0]).ToList();
            var yValues = readings.Select(r => r[1]).ToList();
            var zValues = readings.Select(r => r[2]).ToList();

            return new AccelerometerStatsDto
            {
                AvgX = Math.Round(xValues.Average(), 4),
                AvgY = Math.Round(yValues.Average(), 4),
                AvgZ = Math.Round(zValues.Average(), 4),
                MaxX = Math.Round(xValues.Max(), 4),
                MaxY = Math.Round(yValues.Max(), 4),
                MaxZ = Math.Round(zValues.Max(), 4),
                MinX = Math.Round(xValues.Min(), 4),
                MinY = Math.Round(yValues.Min(), 4),
                MinZ = Math.Round(zValues.Min(), 4),
            };
        }
    }
}
