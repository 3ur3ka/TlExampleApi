
using System.Collections.Generic;
using TlApiExample.Models.Responses;

namespace TlApiExample.Models
{
    public class Cache
    {
        public string Code { get; set; }
        public ExchangeResponseDTO ExchangeResponseDTO { get; set; }
        public AccountsResponseDTO AccountsResponseDTO { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public List<AggregatedTransactionsDTO> AggregatedTransactions { get; set; } = new List<AggregatedTransactionsDTO>();
    }
}
