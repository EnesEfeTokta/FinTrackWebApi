using System;
using System.Net;
using System.Net.Mail;

namespace FinTrackWebApi.Services.EmailService
{
    public class EmailSender : IEmailSender
    {
        private SmtpClient _smtpClient;

        private readonly IConfiguration configuration;

        public EmailSender(IConfiguration configuration)
        {
            this.configuration = configuration;

            _smtpClient = new SmtpClient(configuration["SMTP:Host"], Convert.ToInt16(configuration["SMTP:Port"]));

            _smtpClient.Credentials = new NetworkCredential(configuration["SMTP:NetworkCredentialMail"], configuration["SMTP:NetworkCredentialPassword"]);
            _smtpClient.EnableSsl = true;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(configuration["SMTP:SenderMail"], configuration["SMTP:SenderName"]);
            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = htmlMessage;
            mail.IsBodyHtml = true;

            _smtpClient.Send(mail);

            return Task.CompletedTask;
        }
    }
}