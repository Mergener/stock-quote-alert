using System.Text.Json;

namespace StockQuoteAlert
{
    /// <summary>
    /// User configuration, as specified by the user-provided config.json file.
    /// Each setting from this file is described in the readme.
    /// </summary>
    internal class AppConfig
    {
        #region Monitoring
        public double MonitoringInterval { get; set; } = 10.0;
        public double EmailSpamInterval { get; set; } = 3600.0;
        #endregion

        #region Stock API
        public static readonly string[] SUPPORTED_STOCK_APIS = [ "twelvedata" ];
        public string? StockAPI { get; set; }

        #region Twelve Data
        public string? TwelveDataAPIKey { get; set; }
        #endregion

        #endregion

        #region SMTP
        public string? SMTPUsername { get; set; }
        public string? SMTPPassword { get; set; }
        public string? SMTPHost { get; set; }
        public int SMTPPort { get; set; } = 587;
        public bool SMTPSSL { get; set; } = true;
        #endregion

        #region Email
        public string SenderName { get; set; } = "Stock Quote Alert System";
        public string? RecipientAddress { get; set; }
        public string RecipientName { get; set; } = "";
        public string BuyEmailSubject { get; set; } = "Buy %%STOCK%%!";
        public string? BuyEmailTemplatePath { get; set; } = null;
        public string SellEmailSubject { get; set; } = "Sell %%STOCK%%!";
        public string? SellEmailTemplatePath { get; set; } = null;
        #endregion

        /// <summary>
        /// Loads a configuration file from the specified path.
        /// Throws an exception if loading fails.
        /// </summary>
        public static AppConfig LoadFromFile(string path)
        {
            try
            {
                string jsonString = File.ReadAllText(path);
                var config = JsonSerializer.Deserialize<AppConfig>(jsonString);

                if (config is null)
                {
                    throw new ArgumentException("Invalid JSON format for config file.");
                }

                return config;
            }
            catch (Exception ex) 
            {
                throw new Exception($"Failed to load config file.", ex);
            }
        }
    }
}
