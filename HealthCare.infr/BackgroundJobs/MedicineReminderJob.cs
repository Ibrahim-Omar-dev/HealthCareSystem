using HealthCare.Application.Interfaces;
using HealthCare.Domain.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HealthCare.Infreastructure.BackgroundJobs
{
    public class MedicineReminderJob : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<MedicineReminderJob> _logger;

        public MedicineReminderJob(
            IServiceProvider services,
            ILogger<MedicineReminderJob> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var medicineService = scope.ServiceProvider
                        .GetRequiredService<IMedicineService>();

                    var dueReminders = await medicineService.GetDueRemindersAsync();

                    foreach (var reminder in dueReminders)
                    {
                        // هنا بتبعت الـ Push Notification للموبايل
                        // باستخدام Firebase FCM أو أي service تاني
                        _logger.LogInformation(
                            "🔔 Reminder due: {Name} at {Time} for medicine {Id}",
                            reminder.MedicineName,
                            reminder.ReminderTime,
                            reminder.MedicineId);

                        // TODO: بعت FCM notification هنا
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in MedicineReminderJob");
                }

                // انتظر دقيقة وكرر
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}