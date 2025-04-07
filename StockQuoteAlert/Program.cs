using StockQuoteAlert;
using StockQuoteAlert.Emails;
using StockQuoteAlert.Stocks;

class Program
{
    public record ParsedArgs(string Stock,
                             decimal LowerBound,     
                             decimal UpperBound, 
                             string? ConfigPath);

    static ParsedArgs ParseArgs(string[] args)
    {
        if (args.Length < 3)
        {
            throw new ArgumentException("Usage: stock-alert.exe <stock-name> <lower-bound> <upper-bound> [config-file]");
        }

        return new ParsedArgs(args[0].Trim(),
            decimal.Parse(args[1]),
            decimal.Parse(args[2]),
            args.Length >= 4 ? args[3] : null);
    }

    static async Task Main(string[] args)
    {
        ParsedArgs parsedArgs;
        try
        {
            parsedArgs = ParseArgs(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid arguments.\n{ex.Message}");
            return;
        }

        string configPath = parsedArgs.ConfigPath ?? "config.json";
        AppConfig.LoadFromFile(configPath);
        Console.WriteLine($"Successfully loaded config from {configPath}.");

        var stockProvider = new TwelveDataStockProvider();
        var stockMonitor = new StockMonitor() { 
            StockProvider = stockProvider, 
            LowerBound = parsedArgs.LowerBound, 
            UpperBound = parsedArgs.UpperBound, 
            TargetStock = parsedArgs.Stock 
        };

        var emailClient = new MailkitEmailClient(
            AppConfig.Active.SMTPHost ?? throw new Exception("No SMTP host found in config."),
            AppConfig.Active.SMTPPort,
            AppConfig.Active.SMTPUsername,
            AppConfig.Active.SMTPPassword,
            AppConfig.Active.SMTPSSL
            )
        {
            From = new EmailAddress(
                AppConfig.Active.SMTPFromName,
                AppConfig.Active.SMTPUsername ?? throw new Exception("No SMTP 'from' address found in config.")
            )
        };

        stockMonitor.PriceAboveUpperbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: AppConfig.Active.SMTPToAddress ?? throw new Exception("No SMTP 'to' address found in config."),
                Subject: "Sell a stock!",
                Content: $"Sell {stock}!\nLatest price: {price} (at {DateTime.Now})"
            ));
            Console.WriteLine("Sent sell warning email.");
        };
        stockMonitor.PriceBelowLowerbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: AppConfig.Active.SMTPToAddress ?? throw new Exception("No SMTP 'to' address found in config."),
                Subject: "Buy a stock!",
                Content: $"Buy {stock}!\nLatest price: {price} (at {DateTime.Now})"
            ));
            Console.WriteLine("Sent purchase warning email.");
        };

        while (true)
        {
            _ = stockMonitor.Poll();

            int intervalMs = (int)(AppConfig.Active.MonitoringInterval * 1000.0);
            await Task.Delay(intervalMs);
        }
    }
}