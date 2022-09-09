using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foxminded.Task11
{
    public class Root
    {
        [JsonPropertyName("exchangeRate")]
        public List<ExchangeModel> ExchangeModel { get; set; }
    }
    public class ExchangeModel
    {
        [JsonPropertyName("baseCurrency")]
        public string BaseCurrency { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("saleRateNB")]
        public double SaleRateNB { get; set; }

        [JsonPropertyName("purchaseRateNB")]
        public double PurchaseRateNB { get; set; }

        [JsonPropertyName("saleRate")]
        public double SaleRate { get; set; }

        [JsonPropertyName("purchaseRate")]
        public double PurchaseRate { get; set; }
    }
}
