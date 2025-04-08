﻿using StockQuoteAlert;
using StockQuoteAlert.Emails;
using StockQuoteAlert.Stocks;
using System.Globalization;

class Program
{
    static async Task Main(string[] args)
    {
        ParsedArgs parsedArgs = ParseArgs(args);
        AppConfig config = LoadConfigFromCLIArgs(parsedArgs);

        var stockProvider = SetupStockProvider(config);
        var emailClient = SetupEmailClient(config);
        var stockMonitor = SetupMonitor(parsedArgs,
                                        config,
                                        stockProvider,
                                        emailClient);

        await RunMonitoringLoop(stockMonitor, parsedArgs, config);
    }

    record ParsedArgs(string Stock,
                      decimal LowerBound,
                      decimal UpperBound,
                      string? ConfigPath);

    static ParsedArgs ParseArgs(string[] args)
    {
        if (args.Length < 3)
        {
            throw new InvalidOperationException("Usage: stock-alert.exe <stock-name> <upper-bound> <lower-bound> [config-file]");
        }

        var parsed = new ParsedArgs(args[0].Trim(),
                                    decimal.Parse(args[2], NumberStyles.Number, CultureInfo.InvariantCulture),
                                    decimal.Parse(args[1], NumberStyles.Number, CultureInfo.InvariantCulture),
                                    args.Length >= 4 ? args[3] : null);

        return parsed;
    }

    static AppConfig LoadConfigFromCLIArgs(ParsedArgs args)
    {
        string configPath = args.ConfigPath ?? "config.json";
        var config = AppConfig.LoadFromFile(configPath);
        Console.WriteLine($"Successfully loaded config from {configPath}.");
        return config;
    }

    static IStockProvider SetupStockProvider(AppConfig config)
    {
        if (string.IsNullOrEmpty(config.TwelveDataAPIKey))
        {
            throw new InvalidOperationException("Missing \"TwelveDataAPIKey\" in configuration file.");
        }
        return new TwelveDataStockProvider(config.TwelveDataAPIKey!);
    }

    static IEmailClient SetupEmailClient(AppConfig config)
    {
        return new MailkitEmailClient(
            config.SMTPHost ?? throw new InvalidOperationException("No SMTP host found in config."),
            config.SMTPPort,
            config.SMTPUsername,
            config.SMTPPassword,
            config.SMTPSSL
            )
        {
            From = new EmailAddress(
            config.SMTPFromName,
                config.SMTPUsername ?? throw new Exception("No SMTP 'from' address found in config.")
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
        if (string.IsNullOrEmpty(config.SMTPToAddress))
        {
            throw new InvalidOperationException("No SMTP 'to' address found in config.");
        }

        stockMonitor.PriceAboveUpperbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: config.SMTPToAddress,
                Subject: "Sell a stock!",
                Content: $"Sell {stock}!\nLatest price: {price.ToMoney()} (at {DateTime.Now})"
            ));
            Console.WriteLine("Sent sell alert email.");
        };
        stockMonitor.PriceBelowLowerbound += async (string stock, decimal price) =>
        {
            await emailClient.SendEmail(new SendEmailArgs(
                To: config.SMTPToAddress,
                Subject: "Buy a stock!",
                Content: $"Buy {stock}!\nLatest price: {price.ToMoney()} (at {DateTime.Now})"
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
        Console.WriteLine($"Sell threshold: > {args.UpperBound.ToMoney()}");
        Console.WriteLine($"Buy threshold: < {args.LowerBound.ToMoney()}");
        Console.WriteLine($"Notifications will be sent to {config.SMTPToAddress}");
        Console.WriteLine();

        while (true)
        {
            await stockMonitor.Poll();
            int intervalMs = (int)(config.MonitoringInterval * 1000.0);
            await Task.Delay(intervalMs);
        }
    }
}