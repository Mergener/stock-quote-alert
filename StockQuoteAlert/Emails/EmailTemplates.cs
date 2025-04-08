namespace StockQuoteAlert.Emails
{
    public static class EmailTemplates
    {
        private const string DEFAULT_BUY_EMAIL =
            "<h1>" +
            "   Action suggested" +
            "</h1>" +
            "<p>Dear %%NAME%%,</p>" +
            "<p>Stock <strong>%%STOCK%%</strong> price has gone below <strong>%%LOWERBOUND%%</strong> and is currently at <strong>%%PRICE%%</strong>.</p>" +
            "<p>" +
            "   <strong>" +
            "       We strongly encourage buying it." +
            "   </strong>" +
            "</p>";

        private const string DEFAULT_SELL_EMAIL =
            "<h1>" +
            "   Action suggested" +
            "</h1>" +
            "<p>Dear %%NAME%%,</p>" +
            "<p>Stock <strong>%%STOCK%%</strong> price has gone above <strong>%%UPPERBOUND%%</strong> and is currently at <strong>%%PRICE%%</strong>.</p>" +
            "<p>" +
            "   <strong>" +
            "       We strongly encourage selling it." +
            "   </strong>" +
            "</p>";

        private static string DefaultTemplate(bool buy) => buy ? DEFAULT_BUY_EMAIL : DEFAULT_SELL_EMAIL;

        /// <summary>
        /// Loads a custom email template from a given path.
        /// If the template cannot be read, returns a default template.
        /// </summary>
        /// <param name="templatePath">The template file path.</param>
        /// <param name="buy">True if a 'buy' email template, false if 'sell'.</param>
        /// <returns></returns>
        public static string LoadTemplateFromFile(string? templatePath, bool buy)
        {
            if (string.IsNullOrEmpty(templatePath))
            {
                // When null, assume the user specified no template.
                return DefaultTemplate(buy);
            }

            try
            {
                return File.ReadAllText(templatePath);
            }
            catch
            {
                Console.Error.WriteLine($"Failed to load template from {templatePath}. " +
                    $"Using default template for '{(buy ? "buy" : "sell")}' emails.");
                return DefaultTemplate(buy);
            }
        }

        public static string ApplySubstitutions(string template,
                                                        string recipientName,
                                                        string stock,
                                                        decimal lowerbound,
                                                        decimal upperbound,
                                                        decimal price)
        {
            return template.Replace("%%NAME%%", recipientName)
                           .Replace("%%STOCK%%", stock)
                           .Replace("%%LOWERBOUND%%", lowerbound.ToMoney())
                           .Replace("%%UPPERBOUND%%", upperbound.ToMoney())
                           .Replace("%%PRICE%%", price.ToMoney());
        }
    }
}