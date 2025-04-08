namespace StockQuoteAlert.Stocks
{
    public interface IStockProvider
    {
        /// <summary>
        /// Fetches the latest price for the specified stock, in USD.
        /// </summary>
        public Task<decimal> GetLatestStockPrice(string stockName);
    }
}
