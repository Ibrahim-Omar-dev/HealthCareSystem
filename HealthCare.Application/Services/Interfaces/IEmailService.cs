using System.Threading.Tasks;

namespace HealthCare.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
