using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace StockQuoteAlert.Stocks
{
    public class TwelveDataStockProvider : IStockProvider, IDisposable
    {
        private readonly HttpClient httpClient = new();
        private readonly string apiKey;
        private bool disposedValue;

        public async Task<decimal> GetLatestStockPrice(string stockName, string currency = "USD")
        {
            var conversionTask = GetConversionRate(currency);
            var priceTask = GetLatestStockPriceUSD(stockName);

            await Task.WhenAll(conversionTask, priceTask);

            return priceTask.Result * conversionTask.Result;
        }

        private record class GetPriceResponse([property: JsonPropertyName("price")] string Price);

        private async Task<decimal> GetLatestStockPriceUSD(string stockName)
        {
            var resp = await httpClient.GetAsync($"/price?apikey={apiKey}&symbol={stockName}");
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<GetPriceResponse>();

            // After some API calls to TwelveData, a few responses came with "" in the
            // price field. These requests had no ApiKey, but returned that instead of 401.
            // Because of that, we'll assume empty strings mean "error".
            if (string.IsNullOrEmpty(body?.Price))
            {
                throw new Exception("An error occurred when calling TwelveData API. " +
                    "Check if the API Key is properly set in the config file.");
            }

            // Make sure we parse with "InvariantCulture", otherwise we may get
            // wrong decimal points.
            if (!decimal.TryParse(body?.Price,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture,
                                 out var price))
            {
                throw new Exception($"Invalid price format returned from TwelveData API: '{body?.Price}'");
            }
            return price;
        }

        private record class CurrencyConversionResponse([property: JsonPropertyName("rate")] double Rate);

        private async Task<decimal> GetConversionRate(string currency)
        {
            // Prevent any casing mistakes.
            currency = currency.ToUpperInvariant();

            if (currency == "USD")
            {
                // TwelveData API already returns in USD, no need to
                // request for a conversion rate.
                return 1;
            }

            var resp = await httpClient.GetAsync($"/currency_conversion?apikey={apiKey}&symbol=USD/{currency}");
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<CurrencyConversionResponse>();
            return (decimal)body!.Rate;
        }

        public TwelveDataStockProvider(string apiKey)
        {
            ArgumentNullException.ThrowIfNull(apiKey);

            this.apiKey = apiKey;
            httpClient.BaseAddress = new Uri("https://api.twelvedata.com/");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    httpClient.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
