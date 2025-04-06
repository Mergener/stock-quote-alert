using StockQuoteAlert;

class Program
{
    static void Main(string[] args)
    {
        AppConfig.LoadFromFile(args[0]);
    }
}