namespace StockQuoteAlert.Stocks
{
    internal interface IStockProvider
    {
        public Task<decimal?> GetLatestStockPrice(string stockName);
    }
}
