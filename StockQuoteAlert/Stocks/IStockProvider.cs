namespace StockQuoteAlert.Stocks
{
    public interface IStockProvider
    {
        public Task<decimal> GetLatestStockPrice(string stockName);
    }
}
