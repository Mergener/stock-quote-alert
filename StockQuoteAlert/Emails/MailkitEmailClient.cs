using MailKit.Net.Smtp;
using MimeKit;

namespace StockQuoteAlert.Emails
{
    public class MailkitEmailClient(
        string smtpServer,
        int smtpPort,
        string? smtpUsername = null,
        string? smtpPassword = null,
        bool useSsl = true) : IEmailClient
    {
        public required EmailAddress From { get; init; }

        public async Task SendEmail(SendEmailArgs args)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(From.Name, From.Address));
            message.To.Add(new MailboxAddress(args.To.Name, args.To.Address));
            message.Subject = args.Subject ?? string.Empty;

            message.Body = new TextPart("html")
            {
                Text = args.Content ?? string.Empty
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, useSsl);

            if (!string.IsNullOrEmpty(smtpUsername) && !string.IsNullOrEmpty(smtpPassword))
            {
                await client.AuthenticateAsync(smtpUsername, smtpPassword);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
