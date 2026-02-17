using HealthCare.Domain.Entities;
using HealthCare.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infreastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {

        public DbSet<RefreshToken> RefreshTokens{ get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<SensorMeasurement> Measurements {  get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
