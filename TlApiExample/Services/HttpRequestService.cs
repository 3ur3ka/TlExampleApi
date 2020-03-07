using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TlApiExample.Helpers;
using TlApiExample.Models;
using TlApiExample.Models.Requests;
using TlApiExample.Models.Responses;

namespace TlApiExample.Services
{
    public interface IHttpRequestService
    {
        Task<bool> DoExchangeAsync();
    }

    public class HttpRequestService : IHttpRequestService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ICacheService _cacheService;
        private readonly IOptions<TrueLayerCredentials> _trueLayerCredentials;

        public HttpRequestService(
            IHttpClientFactory clientFactory,
            ICacheService cacheService,
            IOptions<TrueLayerCredentials> trueLayerCredentials
        )
        {
            _clientFactory = clientFactory;
            _cacheService = cacheService;
            _trueLayerCredentials = trueLayerCredentials;
        }

        public async Task<bool> DoExchangeAsync()
        {
            string uri = _trueLayerCredentials.Value.BaseAuthUrl + "/connect/token";

            ExchangeRequestDTO exchangeDTO = new ExchangeRequestDTO()
            {
                GrantType = "authorization_code",
                ClientId = _trueLayerCredentials.Value.ClientId,
                ClientSecret = _trueLayerCredentials.Value.ClientSecret,
                RedirectUri = _trueLayerCredentials.Value.RedirectUrl,
                Code = _cacheService.GetCache().Code
            };

            ExchangeResponseDTO responseObj = (ExchangeResponseDTO)await DoRequest<ExchangeResponseDTO>(exchangeDTO, uri);

            if (responseObj == null)
                return false;

            Cache cache = new Cache { ExchangeResponseDTO = responseObj, Code = null };

            _cacheService.SetCache(cache);

            return true;
        }

        public async Task<IResponse> DoRequest<TResponse>(IRequest request, string uri) where TResponse : IResponse
        {
            try
            {
                using HttpClient client = _clientFactory.CreateClient();

                Dictionary<string, string> dict = request.ToDict();

                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(dict);

                var response = await client.PostAsync(uri, requestContent);
                if (response != null)
                {
                    response.EnsureSuccessStatusCode();
                    var jsonString = await response.Content.ReadAsStringAsync();

                    IResponse responseObj = JsonConvert.DeserializeObject<TResponse>(jsonString);

                    return responseObj;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong when trying to exchange code: " + ex.Message);
            }

            return null;
        }
    }
}
