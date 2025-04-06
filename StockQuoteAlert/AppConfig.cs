using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockQuoteAlert
{
    internal class AppConfig
    {
        #region Twelve Data
        public string? TwelveDataAPIKey { get; set; }
        #endregion

        #region
        public static AppConfig Active { get; private set; } = new();

        public static void LoadFromFile(string path)
        {
            try
            {
                string jsonString = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<AppConfig>(jsonString);

                if (config is null)
                {
                    return;
                }

                Active = config;
            }
            catch (Exception ex) 
            {
                throw new Exception($"Failed to load config file.", ex);
            }
        }
        #endregion
    }
}
