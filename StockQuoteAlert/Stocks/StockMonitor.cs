using StockQuoteAlert.Currencies;

namespace StockQuoteAlert.Stocks
{
    public class StockMonitor
    {
        public event Action<string, decimal>? PriceBelowLowerbound;
        public event Action<string, decimal>? PriceAboveUpperbound;

        public decimal LowerBound { get; init; }
        public decimal UpperBound { get; init; }
        public required string TargetStock { get; init; }
        public required IStockProvider StockProvider { get; init; }
        public required string Currency { get; init; }

        /// <summary>
        /// A currency converter in case the stock provider API returns a currency
        /// different than the one we want to work with.
        /// </summary>
        public required ICurrencyConverter CurrencyConverter { get; init; }

        /// <summary>
        /// An interval, in seconds, to prevent event spamming.
        /// 
        /// The interval works as follows:
        /// If a PriceAboveUpperbound event is fired, no other PriceAboveUpperbound event 
        /// will be fired until either 
        /// 
        /// 1. This interval ends;
        /// or 
        /// 2. The price has dropped below the upperbound since the last event was fired.
        /// 
        /// The equivalent logic also applies to PriceBelowLowerbound emails.
        /// </summary>
        public double EventCooldown { get; set; } = 0;

        private enum PollResult
        {
            None,
            AboveUpperbound,
            BelowLowerbound,
        }
        private PollResult lastPollResult = PollResult.None;
        private DateTime lastPollTime = DateTime.MinValue;

        /// <summary>
        /// Checks stock prices.
        /// Fires PriceAboveUpperbound if a price surpasses the upperbound.
        /// Fires PriceBelowLowerbound if a price drops below the lowerbound.
        /// </summary>
        public async Task Poll()
        {
            try
            {
                var (stockPriceUnconverted, apiCurrency) = await StockProvider.GetLatestStockPrice(TargetStock);
                var conversionRate = await CurrencyConverter.GetConversionRate(apiCurrency, Currency);

                var stockPrice = stockPriceUnconverted * conversionRate;

                DateTime pollTime = DateTime.Now;
                PollResult pollResult = PollResult.None;

                if (stockPrice < LowerBound)
                {
                    pollResult = PollResult.BelowLowerbound;

                    if (pollTime > lastPollTime.AddSeconds(EventCooldown) || lastPollResult != pollResult)
                    {
                        PriceBelowLowerbound?.Invoke(TargetStock, stockPrice);
                    }
                }
                else if (stockPrice > UpperBound)
                {
                    pollResult = PollResult.AboveUpperbound;

                    if (pollTime > lastPollTime.AddSeconds(EventCooldown) || lastPollResult != pollResult)
                    {
                        PriceAboveUpperbound?.Invoke(TargetStock, stockPrice);
                    }
                }

                lastPollResult = pollResult;
                lastPollTime = pollTime;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error while monitoring stocks: {ex.Message}");
            }
        }
    }
}
