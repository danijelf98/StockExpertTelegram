using System;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockExpertTelegram.Services
{
    public class StockAlert
    {
        public string Ticker { get; set; } // Stock ticker symbol, e.g., "AAPL"
        public double TargetPrice { get; set; } // Target price to trigger the alert
        public long ChatId { get; set; }  // Telegram chat ID of the user who set the alert
    }

    public class StockService
    {

        private readonly List<StockAlert> _alerts = new List<StockAlert>();

        // Method to add a new alert
        public void AddAlert(string ticker, double targetPrice, long chatId)
        {
            _alerts.Add(new StockAlert
            {
                Ticker = ticker,
                TargetPrice = targetPrice,
                ChatId = chatId
            });
        }

        // Method to get all alerts
        public List<StockAlert> GetAlerts()
        {
            return _alerts;
        }

        // Method to remove an alert (optional)
        public void RemoveAlert(string ticker, long chatId)
        {
            _alerts.RemoveAll(alert => alert.Ticker == ticker && alert.ChatId == chatId);
        }

        public async Task<double?> GetStockPriceAsync(string ticker)
        {
            try
            {
                Console.WriteLine($"Attempting to fetch stock price for {ticker}");

                // Fetch the latest quote using Yahoo Finance API
                var securities = await Yahoo.Symbols(ticker).Fields(Field.RegularMarketPrice).QueryAsync();

                // Check if the result is valid
                if (securities.TryGetValue(ticker, out var security))
                {
                    Console.WriteLine($"Successfully fetched price for {ticker}: {security[Field.RegularMarketPrice]}");
                    // Access the real-time price
                    return security[Field.RegularMarketPrice];
                }

                Console.WriteLine($"Failed to fetch price for {ticker}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching stock price for {ticker}: {ex.Message}");
                return null;
            }
        }
    }
}

