using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Globalization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;

namespace Foxminded.Task11
{
    public class Program
    {

        public static void Main(string[] args)
        {
            #region Adding appsettings
            using IHost host = Host.CreateDefaultBuilder(args).Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            #endregion

            #region Telegram_bot initialization

            string key = config.GetValue<string>("Key:BotToken");// reading key from appsettings.json
            var botClient = new TelegramBotClient(key);

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.GetUpdatesAsync(offset: -1);
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );
            #endregion       

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

            #region initialize currency codes from appsettings
            using IHost host = Host.CreateDefaultBuilder().Build();
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            var currencies = config.GetSection("Currency:Codes").Get<List<string>>();
            #endregion
            Dictionary<string, string> month = new Dictionary<string, string>()
            {
                {"Jan", "01" }, {"Feb", "02"}, {"Mar", "03"}, {"Apr", "04"},
                {"May", "05" }, {"Jun", "06"}, {"Jul", "07"}, {"Aug", "08"},
                {"Sep", "09" }, {"Oct", "10"}, {"Nov", "11"}, {"Dec", "12"},
            };

            var chatId = message.Chat.Id;

            int temp = 0;
            int.TryParse(message.Text, out temp);

            if (message.Text == "RUB")
            {
                var userId = message.From.Id.ToString();
                MessageHolder.Dictionary.Remove(userId, out List<string> retrievedValue);

                Message sentMessage = await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "You better not joke about it again, I have your id.",
                   replyMarkup: new ReplyKeyboardRemove(),
                   cancellationToken: cancellationToken);
            }

            //Set currency
            else if (!MessageHolder.Dictionary.ContainsKey(message.From.Id.ToString()))
            {
                var userId = message.From.Id.ToString();

                MessageHolder.Dictionary.TryAdd(userId, new List<string>());

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                 new KeyboardButton[]{ "AUD", "CAD", "CZK", "DKK", "HUF" },
                 new KeyboardButton[]{ "ILS", "JPY", "LVL", "LTL", "NOK" },
                 new KeyboardButton[]{ "SKK", "SEK", "CHF", "RUB", "GBP"  },
                 new KeyboardButton[]{ "USD", "BYR", "EUR", "GEL", "PLZ"  },
                })
                {
                    ResizeKeyboard = true
                };

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "This bot was only made for the currency exchange, let's pretend, that it is what you wanted." +
                    "Now you may choose the currency.",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }

            //Set year
            else if (currencies.Contains(message.Text))
            {
                var userId = message.From.Id.ToString();

                MessageHolder.Dictionary[userId].Add(message.Text);

                int earliestPossibleYear = 2014;
                int currentYear = DateTime.Today.Year;
                int rows = ((currentYear - earliestPossibleYear) + 3) / 3; // 3 is the number of buttons in row
                int cols = 3;
                KeyboardButton[][] universalYearLayout = new KeyboardButton[rows][];

                #region Universal layout for year
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (universalYearLayout[i] is null)
                        {
                            if ((currentYear - earliestPossibleYear) + 1 < 3)
                            {
                                universalYearLayout[i] = new KeyboardButton[(currentYear - earliestPossibleYear)];
                            }
                            else
                            {
                                universalYearLayout[i] = new KeyboardButton[3];
                            }
                        }
                        if (earliestPossibleYear <= currentYear)
                        {
                            universalYearLayout[i][j] = earliestPossibleYear.ToString();
                            earliestPossibleYear++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                #endregion
                ReplyKeyboardMarkup replyKeyboardMarkup = new(universalYearLayout)
                {
                    ResizeKeyboard = true
                };

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose a year you want to check",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }

            //Set month
            else if (temp >= 2014 && temp <= DateTime.Today.Year)
            {
                var userId = message.From.Id.ToString();

                MessageHolder.Dictionary[userId].Add(message.Text);

                #region Create univeral layout for months
                //Set an array of a full 12 month
                var monthInYear = new List<string>
                    {"Jan", "Feb", "Mar",
                     "Apr", "May", "Jun",
                     "Jul", "Aug", "Sep",
                     "Oct", "Nov", "Dec"};
                //If user chose a current year, set an array up to the current month
                if (temp == DateTime.Today.Year)
                {
                    monthInYear = new[]
                    {"Jan", "Feb", "Mar",
                     "Apr", "May", "Jun",
                     "Jul", "Aug", "Sep",
                     "Oct", "Nov", "Dec"}
                        .Take(DateTime.Today.Date.Month)
                        .ToList();
                }
                //Count the number of button rows, in order to create a universal layout
                int numberOfRows = (monthInYear.Count + 4 - 1) / 4; // 4 is the number of buttons in row
                KeyboardButton[][] universalLayout = new KeyboardButton[numberOfRows][];

                //Fill the layout according to the chosen year
                for (int i = 0; i < numberOfRows; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        if (universalLayout[i] is null)
                        {
                            if (monthInYear.Count < 4)
                            {
                                universalLayout[i] = new KeyboardButton[monthInYear.Count];
                            }
                            else
                            {
                                universalLayout[i] = new KeyboardButton[4];
                            }
                        }
                        if (monthInYear.Count > 0)
                        {
                            universalLayout[i][j] = monthInYear.ElementAt(0);
                            monthInYear.RemoveAt(0);
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                ReplyKeyboardMarkup replyKeyboardMarkup = new(universalLayout)
                {
                    ResizeKeyboard = true
                };
                #endregion

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose a month you want to check",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            //Set day
            else if (month.ContainsKey(message.Text))
            {
                var userId = message.From.Id.ToString();
                string monthUsingDigit = month[message.Text];

                MessageHolder.Dictionary[userId].Add(monthUsingDigit);

                #region Create universal layout for days
                var messages = MessageHolder.Dictionary[userId];
                //Creating temp year and month variables filling them with the date that were chosen by user,
                //to cover all possible variants and create a correct layout
                var tempYear = int.Parse(messages[1]);
                var tempMonth = int.Parse(monthUsingDigit);
                int daysInChosenMonth = 0;
                //Then we have to check if user chose current month, to limit days to today
                if (DateTime.Today.Year == tempYear && DateTime.Today.Date.Month == tempMonth)
                {
                    daysInChosenMonth = DateTime.Today.Date.Day;
                }
                else
                {
                    daysInChosenMonth = DateTime.DaysInMonth(tempYear, tempMonth);
                }

                int rows = 4; //I want to create a layout with 4 rows 8 buttons max on each
                int cols = 8;
                KeyboardButton[][] universalDayLayout = new KeyboardButton[rows][];

                //Fill the layout according to the chosen year
                int day = 1;
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (universalDayLayout[i] is null)
                        {
                            if ((daysInChosenMonth - day) < 8)
                            {
                                universalDayLayout[i] = new KeyboardButton[(daysInChosenMonth - day) + 1]; //+1 cause days count is started from 1, not from 0
                            }
                            else
                            {
                                universalDayLayout[i] = new KeyboardButton[8];
                            }
                        }

                        if (day <= daysInChosenMonth)
                        {
                            universalDayLayout[i][j] = day.ToString();
                            day++;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                ReplyKeyboardMarkup replyKeyboardMarkup = new(universalDayLayout)
                {
                    ResizeKeyboard = true
                };
                #endregion

                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose a day you want to check",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            else if (temp >= 1 && temp <= 31)
            {
                var userId = message.From.Id.ToString();
                string dayToAdd = "";
                if (int.Parse(message.Text) < 10)
                {
                    dayToAdd = "0" + message.Text;
                }
                else
                {
                    dayToAdd = message.Text;
                }
                MessageHolder.Dictionary[userId].Add(dayToAdd);

                bool validDate = false;
                var messages = MessageHolder.Dictionary[userId];
                var currency = messages[0];
                messages.RemoveAt(0);
                string date = String.Join('.', messages);

                //Check if date is in the right format
                //But first convert in into the appropriate format for privatbank API
                date = ConvertDateTimeFormat(date, "yyyy.MM.dd", "dd.MM.yyyy", null);
                var dateFormats = new[] { "dd.MM.yyyy", "dd-MM-yyyy", "dd/MM/yyyy" };
                DateTime scheduleDate;
                validDate = DateTime.TryParseExact(
                    date,
                    dateFormats,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.None,
                            out scheduleDate);

                if (validDate)
                {
                    var textMessage = string.Empty;
                    try
                    {
                        textMessage = await LoadExchangeRate(currency, date);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    MessageHolder.Dictionary.Remove(userId, out List<string> retrievedValue);

                    message = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: textMessage,
                    replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: cancellationToken);
                }
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
        public static string ConvertDateTimeFormat(string input, string inputFormat, string outputFormat, IFormatProvider culture)
        {
            DateTime dateTime = DateTime.ParseExact(input, inputFormat, culture);
            return dateTime.ToString(outputFormat, culture);
        }
        public static async Task<string> LoadExchangeRate(string currency, string date)
        {
            var rate = await ProcessExchange.LoadExchange(currency, date);
            if (rate == null)
            {
                return $"The bank does not have info about rates of {currency} at the {date}.";
            }
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