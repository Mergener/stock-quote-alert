# StockQuoteAlert

A command-line tool that notifies, via email, whenever a certain stock price surpasses an upperbound or lowerbound.

When the price is above the upperbound, a 'sell' email is sent.
Conversely, when the price drops below the lowerbound, a 'buy' email is sent.

## Prerequisites
[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Building

### Via dotnet CLI

To build the project, navigate to the project directory and run the following command:

```
dotnet build -c <Debug|Release>
```

This will create an executable file in the `bin/Debug|Release/net8.0` directory.

### Via Visual Studio
Simply launching Visual Studio and opening the solution file will handle dependencies and build the project.

Note that you need to specify command line arguments in the project Debug properties in order to properly run the program.

No Visual Studio versions below VS 2022 were tested.

## Usage

Once built `cd` to the executable directory and run:

```bash
StockQuoteAlert <stock-name> <upperbound> <lowerbound> [config]
```

Where:
- `stock-name`: The name of the stock you want to monitor (e.g., `AAPL` for Apple).
- `upperbound`: The upper price limit (in USD) for the stock.
- `lowerbound`: The lower price limit (in USD) for the stock.
- `config`: Optional. The path to the configuration file. If not provided, the program will look for a file named `config.json` in the working directory.

Example:
```bash
StockQuoteAlert AAPL 150 200 path/to/config.json
```

Note that the program **requires** a valid configuration file that
specifies at least some required options. See the section below.

## Configuration

A configuration file is required to execute the program. It can also be used to override some default settings.

A sample configuration file is provided in the repository as `sample-config.json`. 
Create a copy of this file, rename it to sample-config.json and fill in any required fields or desired optional fields.

### Required Fields

- `RecipientAddress` (string): The email address where notifications will be sent.

- `SMTPUsername` (string): Your email address used for sending notifications.

- `SMTPPassword` (string): Your email password. If you are using Gmail, you may need to create an App Password.

- `SMTPHost` (string): The SMTP server address for your email provider. For example, for Gmail, it would be `smtp.gmail.com`.

- `StockAPI` (string): The stock API of choice. Currently supported options are: `"twelvedata"`.

#### Stock API Dependent fields
If you choose the TwelveData API, you must also provide the following field:
- `TwelveDataAPIKey` (string): Your Twelve Data API key. You can obtain one for free at [Twelve Data](https://twelvedata.com/).

### Optional Fields

- `SMTPPort` (int): The SMTP port number. Defaults to `587` if unspecified.

- `SMTPSSL` (bool): Whether to use SSL for the SMTP connection. Defaults to `false` if unspecified.

- `MonitoringInterval` (int): Interval, in seconds, between each stock price check. Defaults to `10` if unspecified.

- `EmailSpamInterval` (int): An interval, in seconds, to prevent email spam. This interval works as follows: if a 'buy' email is sent, no other 'buy' emaill will be sent
until either this interval ends or the price drops below the
upperbound. The equivalent logic also applies to 'sell' emails. Defaults to `3600` (1 hour) if unspecified.

- `BuyEmailTemplatePath` (string): Path to a custom HTML email template for 'buy' emails. If not provided, a default template will be used.

- `BuyEmailSubject` (string): The subject text of 'buy' emails. Defaults to "Buy %%STOCK%%!"  if unspecified.

- `SellEmailTemplatePath` (string): Path to a custom HTML email template for 'sell' emails. If not provided, a default template will be used.

- `SellEmailSubject`: The subject text of 'buy' emails. Defaults to "Sell %%STOCK%%!"  if unspecified.

- `SenderName`: How the sender's name will appear in the email. Defaults to "Stock Quote Alert System" if unspecified.

- `RecipientName`: The name of the email recipient. Defaults to an empty value if unspecified.

## Email Templates

A custom email template can be provided in the configuration file for both 'buy' and 'sell' emails. The template must be an HTML file.
The following placeholders will be replaced with actual values when the email is sent:

- `%%STOCK%%`: The name of the stock being monitored.
- `%%PRICE%%`: The current price of the stock.
- `%%UPPERBOUND%%`: The upper price limit set for the stock.
- `%%LOWERBOUND%%`: The lower price limit set for the stock.
- `%%NAME%%`: The name of the email recipient (as in `RecipientName`)