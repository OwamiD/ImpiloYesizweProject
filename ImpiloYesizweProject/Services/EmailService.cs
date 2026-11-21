using ImpiloYesizweProject.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ImpiloYesizweProject.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody,
            string? textBody = null,
            CancellationToken cancellationToken = default)
        {
            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _settings.SenderName,
                    _settings.SenderEmail));

            message.To.Add(
                MailboxAddress.Parse(recipientEmail));

            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlBody,
                TextBody = textBody
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(
                    _settings.SmtpServer,
                    _settings.Port,
                    SecureSocketOptions.StartTls,
                    cancellationToken);

                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password,
                    cancellationToken);

                await client.SendAsync(
                    message,
                    cancellationToken);
            }
            finally
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync(
                        true,
                        cancellationToken);
                }
            }
        }
    }
}
