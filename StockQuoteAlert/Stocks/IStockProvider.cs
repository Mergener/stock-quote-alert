namespace StockQuoteAlert.Stocks
{
    public interface IStockProvider
    {
        /// <summary>
        /// Fetches the latest price for the specified stock, in the specified currency.
        /// </summary>
        public Task<decimal> GetLatestStockPrice(string stockName, string currency = "USD");
    }
}
