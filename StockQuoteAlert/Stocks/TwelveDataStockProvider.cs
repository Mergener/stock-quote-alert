using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using static System.Net.WebRequestMethods;

namespace StockQuoteAlert.Stocks
{
    internal class TwelveDataStockProvider : IStockProvider, IDisposable
    {
        private readonly HttpClient httpClient = new();
        private readonly string apiKey;
        private bool disposedValue;

        private record class GetPriceResponse([property: JsonPropertyName("price")] string Price);

        public async Task<decimal?> GetLatestStockPrice(string stockName)
        {
            var resp = await httpClient.GetFromJsonAsync<GetPriceResponse>($"/price?apikey={apiKey}&symbol={stockName}");

            // Make sure we parse with "InvariantCulture", otherwise we may get
            // wrong decimal points.
            if (decimal.TryParse(resp?.Price,
                                 NumberStyles.Number,
                                 CultureInfo.InvariantCulture, 
                                 out var price))
            {
                return price;
            }
            return null;
        }

        public TwelveDataStockProvider(string apiKey, string baseUrl = "https://api.twelvedata.com/")
        {
            this.apiKey = apiKey;
            httpClient.BaseAddress = new Uri(baseUrl);
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
