using StockQuoteAlert;
using StockQuoteAlert.Emails;
using StockQuoteAlert.Stocks;
using System.Globalization;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var parsedArgs = ParseArgs(args);
            var config = LoadConfigFromCLIArgs(parsedArgs);
            var stockProvider = SetupStockProvider(config);
            var emailClient = SetupEmailClient(config);
            var stockMonitor = SetupMonitor(parsedArgs,
                                            config,
                                            stockProvider,
                                            emailClient);

            await RunMonitoringLoop(stockMonitor, parsedArgs, config);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal: {ex.Message}");
            return -1;
        }
    }

    record ParsedArgs(string Stock,
                      decimal LowerBound,
                      decimal UpperBound,
                      string? ConfigPath);

    static ParsedArgs ParseArgs(string[] args)
    {
        if (args.Length < 3)
        {
            throw new InvalidOperationException("Incorrect usage. Expected:\n\tstock-alert.exe <stock-name> <upper-bound> <lower-bound> [config-file]");
        }

        try
        {
            var parsed = new ParsedArgs(args[0].Trim(),
                                        decimal.Parse(args[2].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture),
                                        decimal.Parse(args[1].Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture),
                                        args.Length >= 4 ? args[3] : null);

            return parsed;
        }
        catch
        {
            throw new Exception("Invalid upperbound/lowerbound price format.");
        }
    }

    static AppConfig LoadConfigFromCLIArgs(ParsedArgs args)
    {
        try
        {
            var configPath = args.ConfigPath ?? "config.json";
            var config = AppConfig.LoadFromFile(configPath);
            Console.WriteLine($"Successfully loaded config from {configPath}.");
            return config;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load configuration file: {ex.Message}");
        }
    }

    static IStockProvider SetupStockProvider(AppConfig config)
    {
        if (string.IsNullOrEmpty(config.StockAPI))
        {
            throw new InvalidOperationException("A valid stock API must be provided in the 'StockAPI' option" +
                " of the config file. " +
                "Valid options are: " + string.Join(", ", AppConfig.SUPPORTED_STOCK_APIS));
        }

        IStockProvider stockProvider;

        if (config.StockAPI == "twelvedata")
        {
            // Check for API key.
            if (string.IsNullOrEmpty(config.TwelveDataAPIKey))
            {
                throw new InvalidOperationException("Missing API key for TwelveData. " +
                    "Specify one with the 'TwelveDataAPIKey' option in the configuration file.");
            }
            stockProvider = new TwelveDataStockProvider(config.TwelveDataAPIKey);
        }
        else
        {
            throw new InvalidOperationException("Unspecified or unsupported 'StockAPI'. " +
                "Valid options are " + string.Join(", ", AppConfig.SUPPORTED_STOCK_APIS));
        }

        return stockProvider;
    }

    static IEmailClient SetupEmailClient(AppConfig config)
    {
        return new MailkitEmailClient(
            config.SMTPHost ?? throw new InvalidOperationException("No SMTP host found in configuration file."),
            config.SMTPPort,
            config.SMTPUsername,
            config.SMTPPassword,
            config.SMTPSSL
            )
        {
            From = new EmailAddress(
                config.SMTPUsername ?? throw new Exception("No SMTP username found in configuration file."),
                config.SenderName
            )
        };
    }

    static StockMonitor SetupMonitor(ParsedArgs args,
                                     AppConfig config,
                                     IStockProvider stockProvider,
                                     IEmailClient emailClient)
    {
        var stockMonitor = new StockMonitor()
        {
            StockProvider = stockProvider,
            LowerBound = args.LowerBound,
            UpperBound = args.UpperBound,
            TargetStock = args.Stock
        };

        // Validate whether we have a 'To' address set before trying to
        // send emails.
        if (string.IsNullOrEmpty(config.RecipientAddress))
        {
            throw new InvalidOperationException("No SMTP 'to' address found in config.");
        }

        string buyEmailTemplate = EmailTemplates.LoadTemplateFromFile(config.BuyEmailTemplatePath, true);
        string sellEmailTemplate = EmailTemplates.LoadTemplateFromFile(config.SellEmailTemplatePath, false);

        stockMonitor.PriceAboveUpperbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: new EmailAddress(config.RecipientAddress, config.RecipientName),
                Subject: EmailTemplates.ApplySubstitutions(config.SellEmailSubject,
                                                           config.RecipientName ?? string.Empty,
                                                           stock,
                                                           args.LowerBound,
                                                           args.UpperBound,
                                                           price,
                                                           config.Currency),
                Content: EmailTemplates.ApplySubstitutions(sellEmailTemplate,
                                                           config.RecipientName ?? string.Empty,
                                                           stock,
                                                           args.LowerBound,
                                                           args.UpperBound,
                                                           price,
                                                           config.Currency)
            ));
            Console.WriteLine("Sent sell alert email.");
        };
        stockMonitor.PriceBelowLowerbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: new EmailAddress(config.RecipientAddress, config.RecipientName),
                Subject: EmailTemplates.ApplySubstitutions(config.BuyEmailSubject,
                                                           config.RecipientName ?? string.Empty,
                                                           stock,
                                                           args.LowerBound,
                                                           args.UpperBound,
                                                           price,
                                                           config.Currency),
                Content: EmailTemplates.ApplySubstitutions(buyEmailTemplate,
                                                           config.RecipientName ?? string.Empty,
                                                           stock,
                                                           args.LowerBound,
                                                           args.UpperBound,
                                                           price,
                                                           config.Currency)
            ));
            Console.WriteLine($"Sent purchase alert email.");
        };

        stockMonitor.EventCooldown = config.EmailSpamInterval;

        return stockMonitor;
    }

    static async Task RunMonitoringLoop(StockMonitor stockMonitor, 
                                        ParsedArgs args, 
                                        AppConfig config)
    {
        Console.WriteLine($"Monitoring stock {args.Stock}.");
        Console.WriteLine($"Sell threshold: > {args.UpperBound.ToMoney(config.Currency)}");
        Console.WriteLine($"Buy threshold: < {args.LowerBound.ToMoney(config.Currency)}");
        Console.WriteLine($"Notifications will be sent to {config.RecipientAddress}");
        Console.WriteLine();

        while (true)
        {
            await stockMonitor.Poll();
            int intervalMs = (int)(config.MonitoringInterval * 1000.0);
            await Task.Delay(intervalMs);
        }
    }
}