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
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .Property(u => u.Gender)
            .HasConversion<string>();

        modelBuilder.Entity<AppUser>()
            .Property(u => u.BloodType)
            .HasConversion<string>();



        base.OnModelCreating(modelBuilder);
    }
}