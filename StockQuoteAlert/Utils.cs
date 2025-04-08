namespace StockQuoteAlert
{
    public static class Utils
    {
        public static string ToMoney(this decimal value, string currency)
        {
            return $"{value:F3} {currency.ToUpperInvariant()}";
        }
    }
}
