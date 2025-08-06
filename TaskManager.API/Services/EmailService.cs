using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using TaskManager.Services.Interfaces;

namespace TaskManager.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _config = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(Environment.GetEnvironmentVariable("EMAIL_FROM")));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            try
            {
                _logger.LogDebug("Connecting to SMTP server...");
                await smtp.ConnectAsync(Environment.GetEnvironmentVariable("EMAIL_SMTP_SERVER"),int.Parse(Environment.GetEnvironmentVariable("EMAIL_PORT") ?? "587"),SecureSocketOptions.StartTls);
                _logger.LogDebug("Authenticating SMTP client...");
                await smtp.AuthenticateAsync(Environment.GetEnvironmentVariable("EMAIL_USERNAME"),Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
                _logger.LogDebug("Sending email to {ToEmail} with subject '{Subject}'", toEmail, subject);
                await smtp.SendAsync(email);

                _logger.LogDebug("Disconnecting from SMTP server...");
                await smtp.DisconnectAsync(true);

                _logger.LogDebug("Email sent successfully to {ToEmail}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
                throw;
            }
        }
    }
}
