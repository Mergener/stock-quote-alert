using StockQuoteAlert;
using StockQuoteAlert.StockMonitor;
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

        IStockProvider stockProvider = new TwelveDataStockProvider();
        var stockMonitor = new StockMonitor() { 
            StockProvider = stockProvider, 
            LowerBound = parsedArgs.LowerBound, 
            UpperBound = parsedArgs.UpperBound, 
            TargetStock = parsedArgs.Stock 
        };

        stockMonitor.PriceAboveUpperbound += (string stock, decimal price) =>
        {
            Console.WriteLine($"Sell {stock}! Latest price: {price}");
        };
        stockMonitor.PriceBelowLowerbound += (string stock, decimal price) =>
        {
            Console.WriteLine($"Buy {stock}! Latest price: {price}");
        };

        Console.WriteLine($"Monitoring stock {parsedArgs.Stock}.");
        const int INTERVAL_MS = 2000;
        while (true)
        {
            _ = stockMonitor.Poll();
            await Task.Delay(INTERVAL_MS);
        }
    }
}