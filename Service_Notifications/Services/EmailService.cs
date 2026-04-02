using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Service_Notifications.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _configuration["SmtpSettings:SenderName"], 
                _configuration["SmtpSettings:SenderEmail"]));
                
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            
            // Connexion sécurisée au serveur SMTP
            await smtp.ConnectAsync(
                _configuration["SmtpSettings:Server"],
                int.Parse(_configuration["SmtpSettings:Port"]!),
                SecureSocketOptions.StartTls);

            // Authentification
            await smtp.AuthenticateAsync(
                _configuration["SmtpSettings:Username"],
                _configuration["SmtpSettings:Password"]);

            // Envoi
            await smtp.SendAsync(email);
            
            // Déconnexion propre
            await smtp.DisconnectAsync(true);
        }
    }
}