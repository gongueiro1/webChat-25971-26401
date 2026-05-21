using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace webChat.Services
{
    // Esta classe implementa a interface que o Identity procura para enviar emails
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // O teu email do Gmail que vai servir como remetente
            var remetenteEmail = "talkr.admin@gmail.com"; 
            
            // A TUA PASSWORD DE APLICAÇÃO (ver explicação abaixo)
            var remetentePassword = "xzvujdicobdnadeu"; 

            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(remetenteEmail, remetentePassword)
            };

            var mailMessage = new MailMessage(
                from: remetenteEmail,
                to: email,
                subject,
                htmlMessage
            )
            {
                IsBodyHtml = true // Permite que o link seja clicável no email
            };

            await client.SendMailAsync(mailMessage);
        }
    }
}