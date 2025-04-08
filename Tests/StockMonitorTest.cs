using StockQuoteAlert.Currencies;
using StockQuoteAlert.Stocks;

namespace Tests
{
    public class StockMonitorTest
    {
        [Fact]
        public async Task TestEvents()
        {
            var stockProvider = new StockProvider639();
            var currencyConverter = new MockCurrencyConverter();

            const int LOWERBOUND = 4;
            const int UPPERBOUND = 8;
            const string STOCK = "AAPL";

            StockMonitor stockMonitor = new()
            {
                LowerBound = LOWERBOUND,
                UpperBound = UPPERBOUND,
                TargetStock = STOCK,
                StockProvider = stockProvider,
                CurrencyConverter = currencyConverter,
                Currency = "USD"
            };

            int lowerbound = 0;
            int upperbound = 0;

            int expectedLowerbound = 0;
            int expectedUpperbound = 0;

            stockMonitor.PriceBelowLowerbound += (string stock, decimal price) =>
            {
                Assert.True(price < LOWERBOUND);
                Assert.Equal(STOCK, stock);
                lowerbound++;
            };

            stockMonitor.PriceAboveUpperbound += (string stock, decimal price) =>
            {
                Assert.True(price > UPPERBOUND);
                Assert.Equal(STOCK, stock);
                upperbound++;
            };

            // First call returns 6, which is within our expected interval.
            await stockMonitor.Poll();
            Assert.Equal(expectedLowerbound, lowerbound);
            Assert.Equal(expectedUpperbound, upperbound);

            // Now, it should return 3, which is below our lowerbound.
            await stockMonitor.Poll();
            Assert.Equal(++expectedLowerbound, lowerbound);
            Assert.Equal(expectedUpperbound, upperbound);

            // Now, it should return 9, which is above our upperbound.
            await stockMonitor.Poll();
            Assert.Equal(expectedLowerbound, lowerbound);
            Assert.Equal(++expectedUpperbound, upperbound);

            // Sequence now returns to 6. However, we'll set our conversion
            // rate to 0.5. With this conversion rate, we expect the stock
            // price to be 3, which is below our lowerbound.
            currencyConverter.ConversionRate = 0.5m;
            await stockMonitor.Poll();
            Assert.Equal(++expectedLowerbound, lowerbound);
            Assert.Equal(expectedUpperbound, upperbound);
        }

        [Fact]
        public async Task TestCooldown()
        {
            var stockProvider = new MockStockProvider();

            const int LOWERBOUND = 4;
            const int UPPERBOUND = 8;
            const string STOCK = "AAPL";

            var stockMonitor = new StockMonitor()
            {
                LowerBound = LOWERBOUND,
                UpperBound = UPPERBOUND,
                TargetStock = STOCK,
                StockProvider = stockProvider,
                CurrencyConverter = new MockCurrencyConverter(),
                Currency = "USD"
            };

            int lowerbound = 0;
            stockMonitor.PriceBelowLowerbound += (string stock, decimal price) =>
            {
                lowerbound++;
            };

            int upperbound = 0;
            stockMonitor.PriceAboveUpperbound += (string stock, decimal price) =>
            {
                upperbound++;
            };

            stockProvider.StockPrice = 10;

            // Test with no cooldown.
            // Here, every Poll() must invoke its respective event.
            await stockMonitor.Poll();
            Assert.Equal(1, upperbound);
            await stockMonitor.Poll();
            Assert.Equal(2, upperbound);

            stockProvider.StockPrice = 0;

            await stockMonitor.Poll();
            Assert.Equal(1, lowerbound);
            await stockMonitor.Poll();
            Assert.Equal(2, lowerbound);

            // Now, test with 'infinite' cooldown.
            // Events must only be invoked when the stock price
            // changes its place in the interval.
            stockMonitor.EventCooldown = 100000000.0;
            lowerbound = 0;
            upperbound = 0;

            // Setting the stock to a value within the interval must "reset"
            // the cooldown.
            stockProvider.StockPrice = 5;
            stockProvider.Currency = "BRL";
            await stockMonitor.Poll();

            stockProvider.StockPrice = 10;
            await stockMonitor.Poll();
            Assert.Equal(1, upperbound);

            await stockMonitor.Poll();
            Assert.Equal(1, upperbound);

            stockProvider.StockPrice = 0;
            await stockMonitor.Poll();
            Assert.Equal(1, lowerbound);

            await stockMonitor.Poll();
            Assert.Equal(1, lowerbound);
        }

        /// <summary>
        /// A pseudo stock provider that returns the repeating
        /// sequence 6, 3, 9, 6, 3, 9, 6... as the stock price.
        /// </summary>
        class StockProvider639 : IStockProvider
        {
            private static IEnumerator<decimal> GetStockPrice()
            {
                while (true)
                {
                    yield return 6;
                    yield return 3;
                    yield return 9;
                }
            }

            private readonly IEnumerator<decimal> stockEnumerator;

            public StockProvider639()
            {
                stockEnumerator = GetStockPrice();
                stockEnumerator.MoveNext();
            }

            public Task<(decimal, string)> GetLatestStockPrice(string stockName)
            {
                var currentPrice = stockEnumerator.Current;
                stockEnumerator.MoveNext();
                return Task.FromResult((currentPrice, "USD"));
            }
        }

        class MockStockProvider : IStockProvider
        {
            public decimal StockPrice { get; set; }
            public string Currency { get; set; } = "USD";

            public Task<(decimal, string)> GetLatestStockPrice(string stockName)
            {
                return Task.FromResult((StockPrice, Currency));
            }
        }

        class MockCurrencyConverter : ICurrencyConverter
        {
            public decimal ConversionRate { get; set; } = 1;

            public Task<decimal> GetConversionRate(string fromCurrency, string toCurrency)
            {
                return Task.FromResult(ConversionRate);
            }
        }
    }
}
