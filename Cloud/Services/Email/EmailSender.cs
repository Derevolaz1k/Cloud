using Microsoft.AspNetCore.Identity.UI.Services;
using NuGet.Protocol.Plugins;
using System.Net.Mail;
using System.Net;
using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cloud.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private string _smtp = "smtp.yandex.ru";
        private int _smtpPort = 587;
        private string _login = "exampl.test@yandex.ru";//for illustration purposes only
        private string _password = "vcoyuyerccgwvqtg";//for illustration purposes only
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(_smtp, _smtpPort);
                smtpClient.Credentials = new NetworkCredential(_login, _password);
                smtpClient.EnableSsl = true;
                using (MailMessage Mail = new MailMessage())
                {
                    Mail.IsBodyHtml = true;
                    Mail.Subject = subject;
                    Mail.Body = htmlMessage;
                    Mail.To.Add(new MailAddress(email));
                    Mail.From = new MailAddress(_login);
                    smtpClient.Send(Mail);
                }
                return Task.CompletedTask;
            }
            catch(Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
