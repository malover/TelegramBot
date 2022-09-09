using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Foxminded.Task11
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            #region Telegram_bot initialization
            var botClient = new TelegramBotClient("5712033208:AAEhtutzGvKST9Tff_nxmrAS34ARReWQmBw");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
            #endregion

            ApiHelper.InitializeClient();

            Console.ReadLine();
            cts.Cancel();
        }

        #region Telegram methods
        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            //Check if message is the text
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            //Ask for a currency
            var currency = string.Empty;
            string date = string.Empty;
            bool validDate = false;
            #region long regex
            //This regex contains all currency codes available in privatbank api
            Regex regex = new Regex(@"^(AUD|CAD|CZK|DKK|HUF|ILS|JPY|LVL|LTL|NOK|SKK|SEK|CHF|RUB|GBP|USD|BYR|EUR|GEL|PLZ)$");
            #endregion
            var chatId = message.Chat.Id;
            if (currency == "")
            {
                currency = update.Message.Text.ToUpper();
            }

            if (!regex.IsMatch(currency))
            {
                currency = "";
                message = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Incorrect currency, try again.",
                cancellationToken: cancellationToken);
            }

            //Check the date
            if (date == "")
            {
                message = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Please, choose the Date.",
                cancellationToken: cancellationToken);
                date = update.Message.Text;
            }

            //Check if date is in the right format
            var dateFormats = new[] { "dd.MM.yyyy", "dd-MM-yyyy", "dd/MM/yyyy" };
            DateTime scheduleDate;
            validDate = DateTime.TryParseExact(
                date,
                dateFormats,
                DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None,
                out scheduleDate);
            if (!validDate)
            {
                date = "";
                message = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "Incorrect date format, try again.",
                cancellationToken: cancellationToken);
            }

            if (currency != "" && date != "")
            {
                var textMessage = await LoadExchangeRate(currency, date);

                message = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: textMessage,
                cancellationToken: cancellationToken);
            }
        }
        public static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
        #endregion

        public static async Task<string> LoadExchangeRate(string currency, string date)
        {
            var rate = await ProcessExchange.LoadExchange(currency, date);
            string result = $"Base currency: {rate.BaseCurrency}\n" +
                            $"Exchange currency: {rate.Currency}\n" +
                            $"NBU purchase rate: {rate.PurchaseRateNB}\n" +
                            $"NBU seeling rate: {rate.SaleRateNB}\n";
            if (rate.PurchaseRate != 0 && rate.SaleRate != 0)
            {
                result += $"PB purchase rate: {rate.PurchaseRate}\n" +
                          $"PB selling rate: {rate.SaleRate}";
            }
            else
                result += "PB does not perform operations with this currency.";

            return result;
        }
    }
}