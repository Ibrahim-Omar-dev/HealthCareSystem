using HealthCare.Domain.Entities;
using HealthCare.Domain.Entities.Identity;
using HealthCare.Domain.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infreastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<SensorMeasurement> Measurements { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .Property(u => u.Gender)
            .HasConversion<string>();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.BloodType)
            .HasConversion<string>();

        modelBuilder.Entity<SensorMeasurement>(entity =>
        {
            entity.Property(e => e.Depth).HasPrecision(18, 6);
            entity.Property(e => e.Latitude).HasPrecision(18, 6);
            entity.Property(e => e.Longitude).HasPrecision(18, 6);
            entity.Property(e => e.OxygenLevel).HasPrecision(18, 6);
            entity.Property(e => e.Pressure).HasPrecision(18, 6);
            entity.Property(e => e.Temperature).HasPrecision(18, 6);
        });

        base.OnModelCreating(modelBuilder);
    }
}