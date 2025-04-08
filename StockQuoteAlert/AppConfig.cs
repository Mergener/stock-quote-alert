using System.Text.Json;

namespace StockQuoteAlert
{
    /// <summary>
    /// User configuration, as specified by the user-provided config.json file.
    /// </summary>
    internal class AppConfig
    {
        #region Monitoring
        /// <summary>
        /// Interval, in seconds, between each stock API call.
        /// </summary>
        public double MonitoringInterval { get; set; } = 10.0;

        /// <summary>
        /// An interval, in seconds, to prevent email spam.
        /// The interval works as follows:
        /// If a 'buy' email is sent, no other 'buy' emaill will be sent
        /// until either this interval ends or the price drops below the
        /// upperbound. The equivalent logic also applies to 'sell' emails.
        /// </summary>
        public double EmailSpamInterval { get; set; } = 3600.0;
        #endregion

        #region Twelve Data
        public string? TwelveDataAPIKey { get; set; }
        #endregion

        #region SMTP
        public string? SMTPUsername { get; set; }
        public string? SMTPPassword { get; set; }
        public string? SMTPHost { get; set; }
        public string SMTPFromName { get; set; } = string.Empty;

        /// <summary>
        /// Who we are sending emails to.
        /// </summary>
        public string? SMTPToAddress { get; set; }
        
        public string? RecipientName { get; set; }
        public int SMTPPort { get; set; } = 587;
        public bool SMTPSSL { get; set; } = true;
        #endregion

        #region Email
        public string BuyEmailSubject { get; set; } = "Buy a stock!";
        public string? BuyEmailTemplatePath { get; set; } = null;
        public string SellEmailSubject { get; set; } = "Sell a stock!";
        public string? SellEmailTemplatePath { get; set; } = null;
        #endregion

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
