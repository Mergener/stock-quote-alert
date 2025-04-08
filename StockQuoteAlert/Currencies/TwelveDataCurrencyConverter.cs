using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace StockQuoteAlert.Currencies
{
    public class TwelveDataCurrencyConverter : ICurrencyConverter, IDisposable
    {
        private readonly HttpClient httpClient = new();
        private readonly string apiKey;
        private bool disposedValue;

        private record class CurrencyConversionResponse([property: JsonPropertyName("rate")] double Rate);

        public async Task<decimal> GetConversionRate(string fromCurrency,
                                                      string toCurrency)
        {
            // Prevent any casing mistakes.
            fromCurrency = fromCurrency.ToUpperInvariant();
            toCurrency = toCurrency.ToUpperInvariant();

            if (fromCurrency == toCurrency)
            {
                // TwelveData API already returns in USD, no need to
                // request for a conversion rate.
                return 1;
            }

            var resp = await httpClient.GetAsync($"/currency_conversion?apikey={apiKey}&symbol={fromCurrency}/{toCurrency}");
            resp.EnsureSuccessStatusCode();
            var body = await resp.Content.ReadFromJsonAsync<CurrencyConversionResponse>();
            return (decimal)body!.Rate;
        }

        public TwelveDataCurrencyConverter(string apiKey)
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
