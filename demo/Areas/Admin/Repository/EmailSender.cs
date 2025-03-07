using System.Net;
using System.Net.Mail;

namespace demo.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("haole01202@gmail.com", "fntcaucemaahosnu")
            };

            return client.SendMailAsync(
                new MailMessage(from: "haole01202@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}