using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace WebCinema.Utilities
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("ammarfarouk2001@gmail.com", "esdu lvhp srfb kyif"),
            };

            return client.SendMailAsync(new MailMessage(from: "ammarfarouk2001@gmail.com", to: email, subject, htmlMessage)
            {
                IsBodyHtml = true
            });
        }
    }
}
