using Newtonsoft.Json;

namespace TlApiExample.Models
{
    public class AggregatedTransaction
    {
        [JsonProperty("transaction_category")]
        public string TransactionCategory { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }
    }
}
