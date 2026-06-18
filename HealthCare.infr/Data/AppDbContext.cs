using HealthCare.Domain.Entities;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infreastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SensorMeasurement> Measurements { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Device> Devices { get; set; }
    public DbSet<FollowRequest> FollowRequests { get; set; }
    public DbSet<Medicine> Medicines { get; set; }
    public DbSet<UserLocation> UserLocations { get; set; }
    public DbSet<AccelerometerReading> AccelerometerReadings { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);


        builder.Entity<FollowRequest>()
            .HasIndex(f => new { f.SenderId, f.ReceiverId })
            .IsUnique();

        builder.Entity<FollowRequest>()
            .HasOne(f => f.Sender)
            .WithMany()
            .HasForeignKey(f => f.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<FollowRequest>()
            .HasOne(f => f.Receiver)
            .WithMany()
            .HasForeignKey(f => f.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}