namespace StockQuoteAlert.Emails
{
    public record EmailAddress(string Address, string Name);
    public record SendEmailArgs(EmailAddress To, string? Subject, string? Content);

    public interface IEmailClient
    {
        EmailAddress From { get; init; }
        public Task SendEmail(SendEmailArgs args);
    }
}
