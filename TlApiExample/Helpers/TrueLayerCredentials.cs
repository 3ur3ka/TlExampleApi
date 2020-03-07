using System;
namespace TlApiExample.Helpers
{
    public class TrueLayerCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string BaseAuthUrl { get; set; }
        public string BaseDataApiUrl { get; set; }

        public string RedirectUrl { get; set; }
    }
}
