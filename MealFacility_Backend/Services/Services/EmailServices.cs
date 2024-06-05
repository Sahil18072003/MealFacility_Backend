using MealFacility_Backend.Models;
using MimeKit;
using System.Net.Mail;
using MailKit.Net.Smtp;
using MealFacility_Backend.Services.IServices;

namespace MealFacility_Backend.Services.Services
{
    public class EmailServices : IEmailServices
    {
        private readonly IConfiguration configuration;

        public EmailServices(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void SendEmail(Email email)
        {
            var emailMessage = new MimeMessage();

            var from = configuration["EmailSettings:From"];

            emailMessage.From.Add(new MailboxAddress("Meal Facility", from));

            emailMessage.To.Add(new MailboxAddress(email.To, email.To));

            emailMessage.Subject = email.Subject;

            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = string.Format(email.Content)
            };

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                try
                {
                    client.Connect(configuration["EmailSettings:SmtpServer"], 465, true);
                    client.Authenticate(configuration["EmailSettings:From"], configuration["EmailSettings:Password"]);
                    client.Send(emailMessage);
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }
    }
}
