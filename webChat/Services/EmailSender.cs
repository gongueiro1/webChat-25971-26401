using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace webChat.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var remetenteEmail = "talkr.admin@gmail.com";
            var remetentePassword = "gidb bnix yfpb jjxe";

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(remetenteEmail, remetentePassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(remetenteEmail, "Talkr"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }
    }
}