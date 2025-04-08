namespace StockQuoteAlert.Currencies
{
    public interface ICurrencyConverter
    {
        public Task<decimal> GetConversionRate(string fromCurrency, 
                                               string toCurrency);
    }
}
