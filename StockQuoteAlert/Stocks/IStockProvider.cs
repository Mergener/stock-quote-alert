namespace StockQuoteAlert.Stocks
{
    public interface IStockProvider
    {
        /// <summary>
        /// Fetches the latest price for the specified stock and returns a (price, currency) tuple.
        /// </summary>
        public Task<(decimal, string)> GetLatestStockPrice(string stockName);
    }
}
