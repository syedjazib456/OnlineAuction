using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace Online_Auction.Models
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailtoSend = new MimeMessage();
            emailtoSend.From.Add(MailboxAddress.Parse("ballpool009915@gmail.com"));
            emailtoSend.To.Add(MailboxAddress.Parse(email));
            emailtoSend.Subject = subject;
            emailtoSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };
            using(var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("ballpool009915@gmail.com", "dfzq gywh oxjw wnwo");
                emailClient.Send(emailtoSend);
                emailClient.Disconnect(true);
            }

            return Task.CompletedTask;
        }
    }
}
