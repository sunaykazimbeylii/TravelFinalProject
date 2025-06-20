using System.Net;
using System.Net.Mail;
using TravelFinalProject.Interfaces;

namespace TravelFinalProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(IEmailService emailServices, string emailTo, string subject, string body, bool isHtml = false)
        {
            SmtpClient smpt = new SmtpClient(_configuration["Email:Host"], Convert.ToInt32(_configuration["Email:Port"]));
            smpt.EnableSsl = true;
            smpt.Credentials = new NetworkCredential(_configuration["Email:LoginEmail"], _configuration["Email:Password"]);
            MailAddress from = new MailAddress(_configuration["Email:LoginEmail"]);
            MailAddress to = new MailAddress(_configuration[emailTo]);
            MailMessage message = new MailMessage(from, to);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = isHtml;
            await smpt.SendMailAsync(message);
        }

        public Task SendMailAsync(string emailTo, string subject, string body, bool isHtml = false)
        {
            throw new NotImplementedException();
        }
    }
}
