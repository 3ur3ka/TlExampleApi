using Newtonsoft.Json;

namespace TlApiExample.Models
{
    public class Account
    {
        [JsonProperty("account_id")]
        public string AccountId { get; set; }
    }
}
