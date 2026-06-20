using HealthCare.Domain.Enums;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infrastructure.BackgroundJobs
{
    public class AlertCleanupJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertCleanupJob> _logger;

        private readonly TimeSpan _interval = TimeSpan.FromHours(24);

        public AlertCleanupJob(
            IServiceScopeFactory scopeFactory,
            ILogger<AlertCleanupJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanupOldAlertsAsync();
                await Task.Delay(_interval, stoppingToken);
            }
        }

        private async Task CleanupOldAlertsAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoff = DateTime.UtcNow.AddDays(-2);

            var oldAlerts = await context.Alerts
                .Where(a => a.Type != AlertType.Critical && a.CreatedAt < cutoff)
                .ToListAsync();

            if (oldAlerts.Count == 0)
            {
                _logger.LogInformation("[AlertCleanupJob] No old alerts to delete.");
                return;
            }

            context.Alerts.RemoveRange(oldAlerts);
            await context.SaveChangesAsync();

            _logger.LogInformation(
                "[AlertCleanupJob] Deleted {Count} non-critical alerts older than 2 days at {Time}",
                oldAlerts.Count, DateTime.UtcNow);
        }
    }
}