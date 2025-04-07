using System.Text.Json;

namespace StockQuoteAlert
{
    internal class AppConfig
    {
        #region Twelve Data
        public string? TwelveDataAPIKey { get; set; }
        #endregion

        #region SMTP
        public string? SMTPUsername { get; set; }
        public string? SMTPPassword { get; set; }
        public string? SMTPHost { get; set; }
        public string SMTPFromName { get; set; } = string.Empty;
        public string? SMTPToAddress { get; set; }
        public int SMTPPort { get; set; } = 587;
        public bool SMTPSSL { get; set; } = true;
        #endregion

        #region Initialization
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
