public class MedicineReminderNotificationDto
{
    public Guid MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public string ReminderTime { get; set; } = string.Empty;  // "08:00"
    public string Message { get; set; } = string.Empty;
}
