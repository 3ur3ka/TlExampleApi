using Newtonsoft.Json;

namespace TlApiExample.Models.Requests
{
    public class ExchangeRequestDTO :  IRequest
    {
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }

        [JsonProperty("redirect_uri")]
        public string RedirectUri { get; set; }

        [JsonProperty("code")]
        public string Code { get; set; }
    }
}
