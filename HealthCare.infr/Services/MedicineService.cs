using HealthCare.Domain.Entities;
using HealthCare.Domain.Interface;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HealthCare.Infreastructure.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly AppDbContext _context;

        private static readonly TimeZoneInfo EgyptTz =
            TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

        public MedicineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MedicineResponseDto> AddMedicineAsync(Guid userId, AddMedicineDto dto)
        {
            var medicine = new Medicine
            {
                UserId = userId,
                Name = dto.Name.Trim(),
                Dosage = dto.Dosage.Trim(),
                Frequency = dto.Frequency,
                ReminderTimesJson = JsonSerializer.Serialize(dto.ReminderTimes),
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Notes = dto.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Medicines.Add(medicine);
            await _context.SaveChangesAsync();

            return MapToDto(medicine);
        }

        public async Task<IEnumerable<MedicineResponseDto>> GetMyMedicinesAsync(Guid userId)
        {
            var medicines = await _context.Medicines
                .Where(m => m.UserId == userId && m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return medicines.Select(MapToDto);
        }


        public async Task<IEnumerable<MedicineResponseDto>> GetCompletedMedicinesAsync(Guid userId)
        {
            var medicines = await _context.Medicines
                .Where(m => m.UserId == userId && !m.IsActive)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return medicines.Select(MapToDto);
        }


        public async Task<MedicineResponseDto?> GetMedicineByIdAsync(Guid userId, Guid medicineId)
        {
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == medicineId && m.UserId == userId);

            return medicine is null ? null : MapToDto(medicine);
        }


        public async Task<(bool Success, string Message)> UpdateMedicineAsync(
            Guid userId, Guid medicineId, AddMedicineDto dto)
        {
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == medicineId && m.UserId == userId);

            if (medicine is null)
                return (false, "Medicine not found.");

            medicine.Name = dto.Name.Trim();
            medicine.Dosage = dto.Dosage.Trim();
            medicine.Frequency = dto.Frequency;
            medicine.ReminderTimesJson = JsonSerializer.Serialize(dto.ReminderTimes);
            medicine.StartDate = dto.StartDate;
            medicine.EndDate = dto.EndDate;
            medicine.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return (true, "Medicine updated successfully.");
        }


        public async Task<(bool Success, string Message)> DeleteMedicineAsync(
            Guid userId, Guid medicineId)
        {
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == medicineId && m.UserId == userId);

            if (medicine is null)
                return (false, "Medicine not found.");

            _context.Medicines.Remove(medicine);
            await _context.SaveChangesAsync();

            return (true, "Medicine deleted successfully.");
        }


        public async Task<(bool Success, string Message)> MarkAsCompletedAsync(
            Guid userId, Guid medicineId)
        {
            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.Id == medicineId && m.UserId == userId);

            if (medicine is null)
                return (false, "Medicine not found.");

            medicine.IsActive = false;
            await _context.SaveChangesAsync();

            return (true, "Medicine marked as completed.");
        }


        public async Task<IEnumerable<MedicineReminderNotificationDto>> GetDueRemindersAsync()
        {
            var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTz);
            var currentTime = nowEgypt.ToString("HH:mm");

            var activeMedicines = await _context.Medicines
                .Where(m => m.IsActive)
                .ToListAsync();

            var due = new List<MedicineReminderNotificationDto>();

            foreach (var medicine in activeMedicines)
            {
                if (medicine.StartDate.HasValue && nowEgypt.Date < medicine.StartDate.Value.Date)
                    continue;

                if (medicine.EndDate.HasValue && nowEgypt.Date > medicine.EndDate.Value.Date)
                    continue;

                var times = JsonSerializer.Deserialize<List<string>>(medicine.ReminderTimesJson)
                            ?? new List<string>();

                if (times.Contains(currentTime))
                {
                    due.Add(new MedicineReminderNotificationDto
                    {
                        MedicineId = medicine.Id,
                        MedicineName = medicine.Name,
                        Dosage = medicine.Dosage,
                        ReminderTime = currentTime,
                        Message = $"حان وقت دوائك — {medicine.Name} ({medicine.Dosage})"
                    });
                }
            }

            return due;
        }


        private MedicineResponseDto MapToDto(Medicine m)
        {
            var times = JsonSerializer.Deserialize<List<string>>(m.ReminderTimesJson)
                        ?? new List<string>();

            var nowEgypt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTz);
            var currentTime = nowEgypt.ToString("HH:mm");

            var nextReminder = times
                .Where(t => string.Compare(t, currentTime) > 0)
                .OrderBy(t => t)
                .FirstOrDefault()
                ?? times.OrderBy(t => t).FirstOrDefault(); 

            return new MedicineResponseDto
            {
                Id = m.Id,
                Name = m.Name,
                Dosage = m.Dosage,
                Frequency = m.Frequency.ToString(),
                ReminderTimes = times,
                StartDate = m.StartDate,
                EndDate = m.EndDate,
                Notes = m.Notes,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt,
                NextReminder = nextReminder is null ? null : $"Next dose at {nextReminder} (Egypt time)"
            };
        }

    }

}