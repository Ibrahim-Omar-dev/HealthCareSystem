using HealthCare.Domain.Enums;

public class AddMedicineDto
{
    public string Name { get; set; } = string.Empty;
    public string Dosage { get; set; } = string.Empty;
    public MedicineFrequency Frequency { get; set; }

    public List<string> ReminderTimes { get; set; } = new();

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}
