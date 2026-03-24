using HealthCare.Domain.Interface;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace HealthCare.Infreastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendResetPasswordEmailAsync(string toEmail, string resetLink)
        {
            var smtpHost = _config["Email:SmtpHost"]!;
            var smtpPort = int.Parse(_config["Email:SmtpPort"]!);
            var smtpUser = _config["Email:SmtpUser"]!;
            var smtpPass = _config["Email:SmtpPass"]!;
            var fromName = _config["Email:FromName"] ?? "HealthCare";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(smtpUser, fromName),
                Subject = "Reset your password",
                Body = $"""
                          <h2>Password Reset</h2>
                          <p>Click the link below to reset your password. It expires in 1 hour.</p>
                          <a href="{resetLink}">Reset Password</a>
                          <p>If you didn't request this, ignore this email.</p>
                          """,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            await client.SendMailAsync(mail);
        }
    }
}
