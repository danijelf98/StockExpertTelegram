using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using StockExpertTelegram.Services;

namespace StockExpertTelegram
{
    internal class Program
    {
        static TelegramBotClient? botClient;

        //new line
        private static Timer _timer;

        public static void StartPriceCheckService(StockService stockService, TelegramBotClient botClient)
        {
            _timer = new Timer(async _ => await CheckPrices(stockService, botClient), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        private static async Task CheckPrices(StockService stockService, TelegramBotClient botClient)
        {
            foreach (var alert in stockService.GetAlerts())
            {
                var price = await stockService.GetStockPriceAsync(alert.Ticker);
                if (price.HasValue && price.Value >= alert.TargetPrice)
                {
                    // Notify user
                    await botClient.SendMessage(alert.ChatId, $"Alert! {alert.Ticker} has reached ${price.Value:F2}", cancellationToken: default);
                }
            }
        }


        static async Task Main()
        {
            // Bot token
            var botClient = new TelegramBotClient("7655779543:AAGLHl25rn1qDB2eLqUYR_wWOtTnr2sdfWI");
            var stockService = new StockService();
            var messageHandler = new BotMessagesHandler(stockService);

            StartPriceCheckService(stockService, botClient);

            // Display bot info
            var me = await botClient.GetMe();
            Console.WriteLine($"Hello, Friend! \n I am {me.FirstName} and my username is: {me.Username}.\n How can I help you?");

            // Start recieving updates
            var cts = new CancellationTokenSource();
            
            botClient.StartReceiving(
                messageHandler.HandleUpdateAsync,
                messageHandler.HandleErrorAsync,
                new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                cancellationToken: cts.Token
                );

            Console.WriteLine("Bot is up and running. Press Enter to exit...");
            Console.ReadLine();
            cts.Cancel(); // Stop receiving updates when finished
        }
    }
}