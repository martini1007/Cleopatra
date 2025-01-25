using System.Net;
using System.Net.Mail;

namespace SendEmail.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string receptor, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string receptor, string subject, string body)
        {
            var email = _configuration.GetValue<string>("EMAIL_CONFIGURATION:EMAIL")?.Trim();
            var password = _configuration.GetValue<string>("EMAIL_CONFIGURATION:PASSWORD")?.Trim();
            var host = _configuration.GetValue<string>("EMAIL_CONFIGURATION:HOST")?.Trim();
            var port = _configuration.GetValue<int>("EMAIL_CONFIGURATION:PORT");

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(host))
            {
                throw new InvalidOperationException("Email configuration is missing or invalid.");
            }

            using var smtpClient = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(email, password)
            };

            if (string.IsNullOrWhiteSpace(receptor) || !receptor.Contains("@"))
            {
                throw new ArgumentException("Invalid recipient email address.");
            }

            
            subject = subject?.Replace("\"", "").Trim();
            body = body?.Replace("\"", "").Trim();

            var message = new MailMessage
            {
                From = new MailAddress(email),
                Subject = subject,
                Body = body,
                IsBodyHtml = true 
            };

            message.To.Add(receptor.Trim('"'));

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email: {ex.Message}");
            }
        }
    }
}