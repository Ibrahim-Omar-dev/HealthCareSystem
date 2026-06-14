using System.Net;
using System.Net.Mail;

namespace HealthCare.Domain.Interface
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp);
    }

}
