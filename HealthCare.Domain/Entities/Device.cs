using HealthCare.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Device
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string DeviceCode { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public AppUser? User { get; set; }
}