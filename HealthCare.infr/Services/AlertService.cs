using HealthCare.Application.Dto;
using HealthCare.Application.Interfaces;
using HealthCare.Domain.Entities;
using HealthCare.Domain.Enums;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
namespace HealthCare.Infreastructure.Services
{


        public class AlertService : IAlertService
        {
            private readonly AppDbContext _context;

            public AlertService(AppDbContext context)
            {
                _context = context;
            }


            public async Task<IEnumerable<AlertDto>> GetMyAlertsAsync(Guid userId)
            {
                var alerts = await _context.Alerts
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return alerts.Select(MapToDto);
            }

            public async Task<IEnumerable<AlertDto>> GetUnreadAlertsAsync(Guid userId)
            {
                var alerts = await _context.Alerts
                    .Where(a => a.UserId == userId && !a.IsRead)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return alerts.Select(MapToDto);
            }

            public async Task<IEnumerable<AlertDto>> GetCriticalAlertsAsync(Guid userId)
            {
                var alerts = await _context.Alerts
                    .Where(a => a.UserId == userId && a.Type == AlertType.Critical)
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return alerts.Select(MapToDto);
            }


        public async Task MarkAsReadAsync(Guid alertId, Guid userId)
        {
            var alert = await _context.Alerts
                .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);

            if (alert is null) return;

            _context.Alerts.Remove(alert); 
            await _context.SaveChangesAsync();
        }
        public async Task MarkAllAsReadAsync(Guid userId)
        {
            var unread = await _context.Alerts
                .Where(a => a.UserId == userId && !a.IsRead)
                .ToListAsync();

            _context.Alerts.RemoveRange(unread); 
            await _context.SaveChangesAsync();
        }
        public async Task GenerateAlertsFromMeasurementAsync(SensorMeasurement m)
            {
                var alerts = new List<Alert>();

                if (m.bpm > 100)
                    alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Critical, AlertCategory.HeartRate,
                        "High Heart Rate Detected",
                        $"Your heart rate reached {m.bpm} BPM during rest. Please consult a doctor if this persists."));

                else if (m.bpm < 50)
                    alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Critical, AlertCategory.HeartRate,
                        "Low Heart Rate Detected",
                        $"Your heart rate dropped to {m.bpm} BPM. Please seek medical advice."));

                if (m.spo2 < 95)
                    alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Warning, AlertCategory.SpO2,
                        "Low SpO2 Level",
                        $"Blood oxygen dropped to {m.spo2}%. Take deep breaths and monitor."));

                if (m.temp > 38.5)
                    alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Warning, AlertCategory.Temperature,
                        "High Temperature Detected",
                        $"Your temperature is {m.temp}°C. You may have a fever."));

                if (m.fall_detected)
                    alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Critical, AlertCategory.FallDetected,
                        "Fall Detected!",
                        $"A fall has been detected ({m.fall_type}). Are you okay?"));

            if (!alerts.Any())
            {
                alerts.Add(MakeAlert(m.UserId!.Value, AlertType.Info, AlertCategory.HeartRate,
                    "All Good ✅",
                    "Your health measurements are within normal ranges."));
            }
            _context.Alerts.AddRange(alerts);
            await _context.SaveChangesAsync();
        }


            private static Alert MakeAlert(
                Guid userId, AlertType type, AlertCategory category,
                string title, string message) => new()
                {
                    UserId = userId,
                    Type = type,
                    Category = category,
                    Title = title,
                    Message = message
                };

            private static AlertDto MapToDto(Alert a) => new()
            {
                Id = a.Id,
                Title = a.Title,
                Message = a.Message,
                Type = a.Type.ToString(),
                Category = a.Category.ToString(),
                IsRead = a.IsRead,
                CreatedAt = a.CreatedAt,
                TimeAgo = GetTimeAgo(a.CreatedAt)
            };

            private static string GetTimeAgo(DateTime createdAt)
            {
                var diff = DateTime.UtcNow - createdAt;

                if (diff.TotalMinutes < 60)
                    return $"{(int)diff.TotalMinutes} minutes ago";
                if (diff.TotalHours < 24)
                    return $"{(int)diff.TotalHours} hours ago";

                return $"{(int)diff.TotalDays} days ago";
            }
        }
    }

