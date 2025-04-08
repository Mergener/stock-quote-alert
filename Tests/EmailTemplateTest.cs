using StockQuoteAlert;
using StockQuoteAlert.Emails;

namespace Tests
{
    public class EmailTemplateTest
    {
        [Fact]
        public static void TestDefaultTemplates()
        {
            string buyTemplate = EmailTemplates.LoadTemplateFromFile("some-invalid-file-path", true);
            string sellTemplate = EmailTemplates.LoadTemplateFromFile("some-invalid-file-path", false);

            Assert.False(string.IsNullOrEmpty(buyTemplate));
            Assert.False(string.IsNullOrEmpty(sellTemplate));
            Assert.NotEqual(sellTemplate, buyTemplate);
        }

        [Fact]
        public static void TestEmailSubstitutions()
        {
            const string SAMPLE_TEMPLATE = "%%NAME%% %%STOCK%% %%UPPERBOUND%% %%LOWERBOUND%% %%PRICE%%";
            const string NAME = "John Doe";
            const string STOCK = "AAPL";
            const decimal UB = 20;
            const decimal LB = 10;
            const decimal PRICE = 25;

            string formatted = EmailTemplates.ApplySubstitutions(SAMPLE_TEMPLATE,
                                                                 NAME,
                                                                 STOCK,
                                                                 LB,
                                                                 UB,
                                                                 PRICE);

            Assert.Equal($"{NAME} {STOCK} {UB.ToMoney()} {LB.ToMoney()} {PRICE.ToMoney()}", formatted);
        }
    }
}
