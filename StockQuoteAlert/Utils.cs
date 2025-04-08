namespace StockQuoteAlert
{
    internal static class Utils
    {
        public static string ToMoney(this decimal value, string currency = "USD")
        {
            return $"{value:F3} {currency}";
        }
    }
}
