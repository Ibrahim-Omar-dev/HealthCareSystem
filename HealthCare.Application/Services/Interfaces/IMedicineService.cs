namespace HealthCare.Domain.Interface
{
    public interface IMedicineService
    {
        Task<MedicineResponseDto> AddMedicineAsync(Guid userId, AddMedicineDto dto);
        Task<IEnumerable<MedicineResponseDto>> GetMyMedicinesAsync(Guid userId);
        Task<IEnumerable<MedicineResponseDto>> GetCompletedMedicinesAsync(Guid userId);
        Task<MedicineResponseDto?> GetMedicineByIdAsync(Guid userId, Guid medicineId);
        Task<(bool Success, string Message)> UpdateMedicineAsync(Guid userId, Guid medicineId, AddMedicineDto dto);
        Task<(bool Success, string Message)> DeleteMedicineAsync(Guid userId, Guid medicineId);
        Task<(bool Success, string Message)> MarkAsCompletedAsync(Guid userId, Guid medicineId);

        Task<IEnumerable<MedicineReminderNotificationDto>> GetDueRemindersAsync();
    }
}
