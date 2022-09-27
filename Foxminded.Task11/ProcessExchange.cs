using Foxminded.Task11.Policies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Foxminded.Task11
{
    public static class ProcessExchange
    {
        private static HttpClient _client;
        private static ClientPolicy _clientPolicy;

        static ProcessExchange()
        {
            try
            {
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Accept.Clear();
                _client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
                _clientPolicy = new ClientPolicy();
            }
            catch
            {

            }
        }
        public static async Task<ExchangeModel> LoadExchange(string currency, string date)
        {
            string url = "";
            url = $"https://api.privatbank.ua/p24api/exchange_rates?json&date={date}";

            var response = await _clientPolicy.LinearHttpRetry.ExecuteAsync(
            () => _client.GetAsync(url));

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                var exchangeRates = JsonNode.Parse(json)["exchangeRate"];
                var curCourse = exchangeRates.AsArray().Where(i => (string)i["currency"] == currency).FirstOrDefault();

                ExchangeModel model = Newtonsoft.Json.JsonConvert.DeserializeObject<ExchangeModel>(curCourse.ToJsonString());
                return model;
            }
            else
            {
                return new ExchangeModel();
            }
        }
    }
}
