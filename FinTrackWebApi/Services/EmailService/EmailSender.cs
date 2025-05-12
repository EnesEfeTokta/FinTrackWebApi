using System;
using System.Net;
using System.Net.Mail;

namespace FinTrackWebApi.Services.EmailService
{
    public class EmailSender : IEmailSender
    {
        private SmtpClient _smtpClient;
        private readonly ILogger<EmailSender> _logger;

        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
        {
            this.configuration = configuration;

            _smtpClient = new SmtpClient(configuration["SMTP:Host"], Convert.ToInt16(configuration["SMTP:Port"]));

            _smtpClient.Credentials = new NetworkCredential(configuration["SMTP:NetworkCredentialMail"], configuration["SMTP:NetworkCredentialPassword"]);
            _smtpClient.EnableSsl = true;
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(configuration["SMTP:SenderMail"] ?? string.Empty, configuration["SMTP:SenderName"]);
                mail.To.Add(email);
                mail.Subject = subject;
                mail.Body = htmlMessage;
                mail.IsBodyHtml = true;

                _smtpClient.Send(mail);

                _logger.LogInformation("Email sent to {Email} with subject {Subject}", email, subject);
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email} with subject {Subject}", email, subject);
                throw;
            }
        }
    }
}