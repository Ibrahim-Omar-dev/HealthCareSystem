using HealthCare.Application.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace HealthCare.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "Reset Your Password";

            email.Body = new TextPart("html")
            {
                Text = $@"
                    <div style='font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:20px;'>
                        <h2 style='color:#4F46E5;'>Password Reset Request</h2>
                        <p>You requested to reset your password. Click the button below:</p>
                        <a href='{resetLink}' style='
                            background-color:#4F46E5;
                            color:white;
                            padding:12px 24px;
                            text-decoration:none;
                            border-radius:6px;
                            display:inline-block;
                            margin:16px 0;
                            font-weight:bold;
                        '>Reset Password</a>
                        <p>This link will expire in <strong>1 hour</strong>.</p>
                        <p style='color:#888;font-size:13px;'>If you did not request this, please ignore this email.</p>
                    </div>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["Email:Host"],
                int.Parse(_config["Email:Port"]!),
                SecureSocketOptions.StartTls
            );
            await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}