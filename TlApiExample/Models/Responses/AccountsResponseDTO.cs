using System.Collections.Generic;
using Newtonsoft.Json;

namespace TlApiExample.Models.Responses
{
    [JsonObject]
    public class AccountsResponseDTO : IResponse
    {
        [JsonProperty("results")]
        public List<Account> Accounts { get; set; } = new List<Account>();
    }
}

