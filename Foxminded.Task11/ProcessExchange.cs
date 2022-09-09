using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Foxminded.Task11
{
    public static class ProcessExchange
    {
        public static async Task<ExchangeModel> LoadExchange(string currency, string date)
        {
            string url = "";
            url = $"https://api.privatbank.ua/p24api/exchange_rates?json&date={date}";

            var streamTask = ApiHelper.ApiClient.GetStreamAsync("https://api.privatbank.ua/p24api/exchange_rates?json&date=01.12.2014");
            var deserialized = await JsonSerializer.DeserializeAsync<Root>(await streamTask);
            var rate = deserialized.ExchangeModel.FirstOrDefault(r => r.Currency == "currency");
            return rate;
        }
    }
}
