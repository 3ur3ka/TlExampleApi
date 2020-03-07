using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TlApiExample.Models.Responses
{
    [JsonObject]
    public class TransactionsResponseDTO : IResponse
    {
        [JsonProperty("results")]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class RunningBalance
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
    }

    public class Meta
    {
        [JsonProperty("bank_transaction_id")]
        public string BankTransactionId { get; set; }

        [JsonProperty("provider_transaction_category")]
        public string ProviderTransactionCategory { get; set; }
    }

    public class Transaction
    {
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("transaction_type")]
        public string TransactionType { get; set; }

        [JsonProperty("transaction_category")]
        public string TransactionCategory { get; set; }

        [JsonProperty("transaction_classification")]
        public List<string> TransactionClassification { get; set; } = new List<string>();

        [JsonProperty("merchant_name")]
        public string MerchantName { get; set; }

        [JsonProperty("running_balance")]
        public RunningBalance RunningBalance { get; set; }

        [JsonProperty("meta")]
        public Meta Meta { get; set; }
    }
}
