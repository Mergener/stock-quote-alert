using System.Net.Mail;

namespace StockQuoteAlert.Emails
{
    public record EmailAddress(string Address, string Name);
    public record SendEmailArgs(string To, string? Subject, string? Content);

    public interface IEmailClient
    {
        EmailAddress From { get; init; }
        public Task SendEmail(SendEmailArgs args);
    }
}
