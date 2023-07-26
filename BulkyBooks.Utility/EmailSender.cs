using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            /*
             * Email Sending Functionality NEEDS to changed by adding
             * real time username and password for smtp client
             * 
            var emailToSend = new MimeMessage();
            emailToSend.From.Add(MailboxAddress.Parse("hello@company.com"));
            emailToSend.To.Add(MailboxAddress.Parse(email));
            emailToSend.Subject = subject;
            emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };

            //Send Email
            using (var emailClient = new SmtpClient())
            {
                emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                emailClient.Authenticate("company@gmail.com", "DotNet!");
                emailClient.Send(emailToSend);
                emailClient.Disconnect(true);
            }
            */
            return Task.CompletedTask;
        }
    }
}
