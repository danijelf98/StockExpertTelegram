using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using YahooFinanceApi;

namespace StockExpertTelegram.Services
{
    public class BotMessagesHandler
    {
        private readonly StockService _stockService;
        public BotMessagesHandler(StockService stockService)
        {
            _stockService = stockService;
        }

        // Method to handle updates (messages, commands) sent to the bot
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text.ToLower();  // Convert text to lowercase for easy comparison

                Console.WriteLine($"Received a message from {chatId}: {messageText}");

                if (messageText.StartsWith("/price"))
                {
                    // Extract ticker symbol from the message (e.g., "/price NVDA")
                    var parts = messageText.Split(' ');
                    if (parts.Length == 2)
                    {
                        var ticker = parts[1].ToUpper();
                        var price = await _stockService.GetStockPriceAsync(ticker);

                        if (price.HasValue)
                        {
                            await botClient.SendMessage(chatId, $"The current price of {ticker} is ${price.Value:F2}.", cancellationToken: cancellationToken);
                        }
                        else
                        {
                            await botClient.SendMessage(chatId, $"Could not retrieve price for {ticker}. Please check the ticker symbol.", cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Usage: /price <ticker> (e.g., /price NVDA)", cancellationToken: cancellationToken);
                    }
                }
                else if (messageText == "/help")
                {
                    string helpMessage = "Welcome to StockExpert Bot! Here are the commands you can use:\n" +
                                         "/price <ticker> - Get the current price for a stock (e.g., /price NVDA)\n" +
                                         "/setalert <ticker> <price> - Set an alert for when a stock reaches a certain price\n" +
                                         "/removealert <ticker> - Remove an existing price alert for a stock\n" +
                                         "/help - Show available commands and usage.";

                    await botClient.SendMessage(chatId, helpMessage, cancellationToken: cancellationToken);
                }

                else if (messageText.StartsWith("/setalert"))
                {
                    var parts = messageText.Split(' ');
                    if (parts.Length == 3 && double.TryParse(parts[2], out double targetPrice))
                    {
                        var ticker = parts[1].ToUpper();
                        _stockService.AddAlert(ticker, targetPrice, chatId);
                        await botClient.SendMessage(chatId, $"Alert set for {ticker} at ${targetPrice:F2}.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Usage: /setalert <ticker> <price> (e.g., /setalert NVDA 250.00)", cancellationToken: cancellationToken);
                    }
                }
                else if (messageText.StartsWith("/removealert"))
                {
                    var parts = messageText.Split(' ');
                    if (parts.Length == 2)
                    {
                        var ticker = parts[1].ToUpper();

                        // Call the RemoveAlert method to remove the alert for this ticker and chat ID
                        _stockService.RemoveAlert(ticker, chatId);

                        await botClient.SendMessage(chatId, $"Alert for {ticker} has been removed.", cancellationToken: cancellationToken);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Usage: /removealert <ticker> (e.g., /removealert NVDA)", cancellationToken: cancellationToken);
                    }
                }

                else
                {
                    await botClient.SendMessage(chatId, "Unknown command. Type /help to see available commands.", cancellationToken: cancellationToken);
                }

            }

        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"An error occurred: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
