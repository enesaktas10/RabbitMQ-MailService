using System.Net.Mail;
using System.Net;


namespace ProductAPI.Services
{
    public class EmailService
    {
        

        private readonly string smtpHost;
        private readonly int smtpPort;
        private readonly string fromEmail;
        private readonly string appPassword;

        

        public EmailService(IConfiguration configuration)
        {
            smtpHost = configuration["EmailSettings:SmtpHost"];
            smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"]);
            fromEmail = configuration["EmailSettings:FromEmail"];
            appPassword = configuration["EmailSettings:AppPassword"];
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(fromEmail, appPassword);

                using (var message = new MailMessage(fromEmail, toEmail))
                {
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true; // HTML mail göndermek için true yapabilirsin.

                    try
                    {
                        await client.SendMailAsync(message);
                        Console.WriteLine($"Mail {toEmail} adresine başarıyla gönderildi.");
                        
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Mail gönderilemedi. Hata: {ex.Message}");
                    }
                }
            }
        }
    }
}
