using StockQuoteAlert.StockMonitor;
using StockQuoteAlert.Stocks;

namespace Tests
{
    public class StockMonitorTest
    {
        /// <summary>
        /// A pseudo stock provider that returns the repeating
        /// sequence 6, 3, 9, null, 6, 3, 9, null, 6... as the stock price.
        /// </summary>
        class StockProvider639 : IStockProvider
        {
            private static IEnumerator<decimal?> GetStockPrice()
            {
                while (true)
                {
                    yield return 6;
                    yield return 3;
                    yield return 9;
                    yield return null;
                }
            }

            private readonly IEnumerator<decimal?> stockEnumerator;

            public StockProvider639()
            {
                stockEnumerator = GetStockPrice();
                stockEnumerator.MoveNext();
            }

            public Task<decimal?> GetLatestStockPrice(string stockName)
            {
                var currentPrice = stockEnumerator.Current;
                stockEnumerator.MoveNext();
                return Task.FromResult(currentPrice);
            }
        }

        [Fact]
        public async Task TestStockMonitor()
        {
            IStockProvider stockProvider = new StockProvider639();

            const int LOWERBOUND = 4;
            const int UPPERBOUND = 8;
            const string STOCK = "AAPL";

            StockMonitor stockMonitor = new()
            {
                LowerBound = LOWERBOUND,
                UpperBound = UPPERBOUND,
                TargetStock = STOCK,
                StockProvider = stockProvider,
            };

            bool calledLowerbound = false;
            bool calledUpperbound = false;

            stockMonitor.PriceBelowLowerbound += (string stock, decimal price) => {
                Assert.True(price < LOWERBOUND);
                Assert.Equal(STOCK, stock);
                calledLowerbound = true;
            };

            stockMonitor.PriceAboveUpperbound += (string stock, decimal price) => {
                Assert.True(price > UPPERBOUND);
                Assert.Equal(STOCK, stock);
                calledUpperbound = true;
            };

            // First call returns 6, which is inside our expected interval.
            await stockMonitor.Poll();
            Assert.False(calledLowerbound);
            Assert.False(calledUpperbound);

            // Now, it should return 3, which is below our lowerbound.
            await stockMonitor.Poll();
            Assert.True(calledLowerbound);
            Assert.False(calledUpperbound);
            calledLowerbound = false;

            // Now, it should return 9, which is above our upperbound.
            await stockMonitor.Poll();
            Assert.False(calledLowerbound);
            Assert.True(calledUpperbound);
            calledUpperbound = false;

            // Now, it should return null.
            // This must be gracefully handled and no callbacks should be called.
            await stockMonitor.Poll();
            Assert.False(calledLowerbound);
            Assert.False(calledUpperbound);
        }
    }
}