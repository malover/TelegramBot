using System.Globalization;
using System.Text;
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
        public static void Main(string[] args)
        {

            #region Telegram_bot initialization
            var botClient = new TelegramBotClient("5712033208:AAEhtutzGvKST9Tff_nxmrAS34ARReWQmBw");

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
            //Api client initialization
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

            #region long regex
            //This regex contains all currency codes available in privatbank api
            Regex regexCurrency = new Regex(@"^(AUD|CAD|CZK|DKK|HUF|ILS|JPY|LVL|LTL|NOK|SKK|SEK|CHF|GBP|USD|BYR|EUR|GEL|PLZ)$");
            #endregion
            Dictionary<string, string> month = new Dictionary<string, string>()
            {
                {"January", "01" }, {"February", "02"}, {"March", "03"}, {"April", "04"},
                {"May", "05" }, {"June", "06"}, {"July", "07"}, {"August", "08"},
                {"September", "09" }, {"October", "10"}, {"November", "11"}, {"December", "12"},
            };

            var chatId = message.Chat.Id;

            int temp = 0;
            int.TryParse(message.Text, out temp);

            if (message.Text == "RUB")
            {
                Message sentMessage = await botClient.SendTextMessageAsync(
                   chatId: chatId,
                   text: "You better not joke about it again, I have your id.",
                   replyMarkup: new ReplyKeyboardRemove(),
                   cancellationToken: cancellationToken);
            }

            //Set currency
            else if (!regexCurrency.IsMatch(message.Text) && temp == 0 && !month.ContainsKey(message.Text))
            {
                var userId = message.From.Id.ToString();

                if (MessageHolder.Dictionary.ContainsKey(userId))
                {
                    MessageHolder.Dictionary[userId] = new List<string>();
                }
                else
                {
                    MessageHolder.Dictionary.TryAdd(userId, new List<string>());
                }

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
            else if (regexCurrency.IsMatch(message.Text))
            {               
                var userId = message.From.Id.ToString();

                if (MessageHolder.Dictionary.ContainsKey(userId))
                {
                    MessageHolder.Dictionary[userId].Add(message.Text);
                }
                else
                {
                    Message errorMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Something went wrong, please, try again.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                 new KeyboardButton[]{ "2014", "2015", "2016"},
                 new KeyboardButton[]{ "2017", "2018", "2019"},
                 new KeyboardButton[]{ "2020", "2021", "2022"},
                })
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
            else if (temp >= 2014 && temp <= 2022)
            {               
                var userId = message.From.Id.ToString();

                if (MessageHolder.Dictionary.ContainsKey(userId))
                {
                    MessageHolder.Dictionary[userId].Add(message.Text); 
                }
                else
                {
                    Message errorMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Something went wrong, please, try again.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }


                #region Layount for the current year
                ReplyKeyboardMarkup layout2022 = new(new[]
                {
                 new KeyboardButton[]{ "January", "February", "March", "April"},
                 new KeyboardButton[]{ "April", "June", "July", "August"},
                 new KeyboardButton[]{ "September"},
                })
                {
                    ResizeKeyboard = false
                };
                #endregion

                ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                {
                 new KeyboardButton[]{ "January", "February", "March", "April"},
                 new KeyboardButton[]{ "April", "June", "July", "August"},
                 new KeyboardButton[]{ "September", "October", "November", "December"},
                })
                {
                    ResizeKeyboard = true
                };

                if (temp == 2022)
                {
                    replyKeyboardMarkup = layout2022;
                }

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

                if (MessageHolder.Dictionary.ContainsKey(userId))
                {
                    MessageHolder.Dictionary[userId].Add(monthUsingDigit);
                }
                else
                {
                    Message errorMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Something went wrong, please, try again.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }

                #region Different layouts for months
                ReplyKeyboardMarkup layout31 = new(new[]
                {
                 new KeyboardButton[]{ "01", "02", "03", "04"},
                 new KeyboardButton[]{ "05", "06", "07", "08"},
                 new KeyboardButton[]{ "09", "10", "11", "12"},
                 new KeyboardButton[]{ "13", "14", "15", "16"},
                 new KeyboardButton[]{ "17", "18", "19", "20"},
                 new KeyboardButton[]{ "21", "22", "23", "24"},
                 new KeyboardButton[]{ "25", "26", "27", "28"},
                 new KeyboardButton[]{ "29", "30", "31"},
                })
                {
                    ResizeKeyboard = false
                };

                ReplyKeyboardMarkup layout30 = new(new[]
                {
                 new KeyboardButton[]{ "01", "02", "03", "04"},
                 new KeyboardButton[]{ "05", "06", "07", "08"},
                 new KeyboardButton[]{ "09", "10", "11", "12"},
                 new KeyboardButton[]{ "13", "14", "15", "16"},
                 new KeyboardButton[]{ "17", "18", "19", "20"},
                 new KeyboardButton[]{ "21", "22", "23", "24"},
                 new KeyboardButton[]{ "25", "26", "27", "28"},
                 new KeyboardButton[]{ "29", "30"},
                })
                {
                    ResizeKeyboard = false
                };

                ReplyKeyboardMarkup layout28 = new(new[]
                {
                 new KeyboardButton[]{ "01", "02", "03", "04"},
                 new KeyboardButton[]{ "05", "06", "07", "08"},
                 new KeyboardButton[]{ "09", "10", "11", "12"},
                 new KeyboardButton[]{ "13", "14", "15", "16"},
                 new KeyboardButton[]{ "17", "18", "19", "20"},
                 new KeyboardButton[]{ "21", "22", "23", "24"},
                 new KeyboardButton[]{ "25", "26", "27", "28"},
                })
                {
                    ResizeKeyboard = false
                };

                ReplyKeyboardMarkup layout29 = new(new[]
                {
                 new KeyboardButton[]{ "01", "02", "03", "04"},
                 new KeyboardButton[]{ "05", "06", "07", "08"},
                 new KeyboardButton[]{ "09", "10", "11", "12"},
                 new KeyboardButton[]{ "13", "14", "15", "16"},
                 new KeyboardButton[]{ "17", "18", "19", "20"},
                 new KeyboardButton[]{ "21", "22", "23", "24"},
                 new KeyboardButton[]{ "25", "26", "27", "28"},
                 new KeyboardButton[]{ "29"},
                })
                {
                    ResizeKeyboard = false
                };
                #endregion
                //Temporal epty layout
                ReplyKeyboardMarkup replyKeyboardMarkup = new ReplyKeyboardMarkup(new KeyboardButton[] { })
                {
                    ResizeKeyboard = false
                };
                if (message.Text == "November" || message.Text == "September" || message.Text == "June" || message.Text == "April")
                {
                    replyKeyboardMarkup = layout30;
                }
                else if (message.Text == "February")
                {
                    replyKeyboardMarkup = layout28;
                }
                else
                {
                    replyKeyboardMarkup = layout31;
                }


                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Choose a day you want to check",
                    replyMarkup: replyKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            else if (temp >= 1 && temp <= 31)
            {
                var userId = message.From.Id.ToString();

                if (MessageHolder.Dictionary.ContainsKey(userId))
                {
                    MessageHolder.Dictionary[userId].Add(message.Text);
                }
                else
                {
                    Message errorMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Something went wrong, please, try again.",
                        replyMarkup: new ReplyKeyboardRemove(),
                        cancellationToken: cancellationToken);
                }

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

                    message = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: textMessage,
                    replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: cancellationToken);
                }
                else
                {
                    message = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "Something went wrong, you should try again. Don't forget, that bot can't see the future, so enter the date in the past.",
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