using StockQuoteAlert.Stocks;

namespace StockQuoteAlert.StockMonitor
{
    public class StockMonitor
    {
        public event Action<string, decimal>? PriceBelowLowerbound;
        public event Action<string, decimal>? PriceAboveUpperbound;

        public decimal LowerBound { get; init; }
        public decimal UpperBound { get; init; }
        public required string TargetStock { get; init; }
        public required IStockProvider StockProvider { get; init; }

        public async Task Poll()
        {
            var stockPrice = await StockProvider.GetLatestStockPrice(TargetStock);
            if (stockPrice == null)
            {
                return;
            }

            if (stockPrice < LowerBound)
            {
                PriceBelowLowerbound?.Invoke(TargetStock, (decimal)stockPrice);
            }
            else if (stockPrice > UpperBound) {
            {
                PriceAboveUpperbound?.Invoke(TargetStock, (decimal)stockPrice);
            }
        }
    }
}
