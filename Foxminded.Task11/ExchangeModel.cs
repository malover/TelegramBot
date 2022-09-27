using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foxminded.Task11
{
    public class ExchangeModel
    {
        public string BaseCurrency { get; set; }
        public string Currency { get; set; }
        public double SaleRateNB { get; set; }
        public double PurchaseRateNB { get; set; }
        public double SaleRate { get; set; }
        public double PurchaseRate { get; set; }
    }
}
