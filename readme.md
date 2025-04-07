# StockQuoteAlert

A command-line tool that notifies, via email, whenever a certain stock price surpasses an upperbound or lowerbound.

When the price is above the upperbound, a 'sell' email is sent.
Conversely, when the price drops below the lowerbound, a 'buy' email is sent.

## Prerequisites
[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Usage

Once built, run:

```bash
StockQuoteAlert <stock-name> <upperbound> <lowerbound> [config]
```

Where:
- `stock-name`: The name of the stock you want to monitor (e.g., `AAPL` for Apple).
- `lowerbound`: The lower price limit for the stock.
- `upperbound`: The upper price limit for the stock.
- `config`: Optional. The path to the configuration file. If not provided, the program will look for a file named `config.json` in the working directory.

Example:
```bash
StockQuoteAlert AAPL 150 200 path/to/config.json
```

## Configuration

A configuration file is required to execute the program. It can also be used to override some default settings.

A sample configuration file is provided in the repository as `sample-config.json`. 
Create a copy of this file, rename it to sample-config.json and fill in any required fields.

### Required Fields

- `TwelveDataAPIKey` (string): Your Twelve Data API key. You can obtain one for free at [Twelve Data](https://twelvedata.com/).

- `SMTPUsername` (string): Your email address used for sending notifications.

- `SMTPPassword` (string): Your email password. If you are using Gmail, you may need to create an App Password.

- `SMTPHost` (string): The SMTP server address for your email provider. For example, for Gmail, it would be `smtp.gmail.com`.

- `SMTPToAddress` (string): The email address where notifications will be sent.

### Optional Fields

- `SMTPPort` (int): The SMTP port number. Defaults to `587` if unspecified.

- `SMTPSSL` (bool): Whether to use SSL for the SMTP connection. Defaults to `false` if unspecified.

- `MonitoringInterval` (int): Interval, in seconds, between each stock price check. Defaults to `10` if unspecified.