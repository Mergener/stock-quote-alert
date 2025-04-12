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

        private record class GetQuoteResponse([property: JsonPropertyName("close")] string Close,
                                              [property: JsonPropertyName("currency")] string Currency);

        public async Task<(decimal, string)> GetLatestStockPrice(string stockName)
        {
            var resp = await httpClient.GetAsync($"/quote?apikey={apiKey}&symbol={stockName}");
            resp.EnsureSuccessStatusCode();

            var body = await resp.Content.ReadFromJsonAsync<GetQuoteResponse>();

            // After some API calls to TwelveData, a few responses came with "" in the
            // price field. These requests had no ApiKey, but returned that instead of 401.
            // Because of that, we'll assume empty strings mean "error".
            if (string.IsNullOrEmpty(body?.Close))
            {
                throw new Exception("An error occurred when calling TwelveData API. " +
                    "Check if the API Key is properly set in the config file.");
            }

            // Make sure we parse with "InvariantCulture", otherwise we may get
            // wrong decimal points.
            if (!decimal.TryParse(body?.Close,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture,
                                 out var price))
            {
                throw new Exception($"Invalid price format returned from TwelveData API: '{body?.Close}'");
            }
            return (price, body.Currency.ToUpperInvariant());
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
