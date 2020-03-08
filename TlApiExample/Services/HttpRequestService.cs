using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        Task<bool> GetAccountsAsync();
        Task<bool> GetTransactionsAsync();
        Task<bool> AggregateAsync();
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

        // Exchange the code for a jwt access token
        public async Task<bool> DoExchangeAsync()
        {
            Cache cache = _cacheService.GetCache();

            if (cache == null || string.IsNullOrEmpty(cache.Code))
                return false;

            string uri = _trueLayerCredentials.Value.BaseAuthUrl + "/connect/token";

            ExchangeRequestDTO exchangeDTO = new ExchangeRequestDTO()
            {
                GrantType = "authorization_code",
                ClientId = _trueLayerCredentials.Value.ClientId,
                ClientSecret = _trueLayerCredentials.Value.ClientSecret,
                RedirectUri = _trueLayerCredentials.Value.RedirectUrl,
                Code = _cacheService.GetCache().Code
            };

            ExchangeResponseDTO responseObj = (ExchangeResponseDTO)await DoRequest<ExchangeResponseDTO>(HttpMethod.Post, uri, false, exchangeDTO);

            if (responseObj == null)
                return false;

            cache = new Cache { ExchangeResponseDTO = responseObj, Code = null };

            _cacheService.SetCache(cache);

            return true;
        }

        // Get the users accounts
        public async Task<bool> GetAccountsAsync()
        {
            if (!IsExchanged())
                return false;

            string uri = _trueLayerCredentials.Value.BaseDataApiUrl + "/data/v1/accounts";

            AccountsResponseDTO responseObj =
                (AccountsResponseDTO)await DoRequest<AccountsResponseDTO>(HttpMethod.Get, uri, true, null);

            if (responseObj == null)
                return false;

            Cache cache = new Cache
            {
                AccountsResponseDTO = responseObj,
                ExchangeResponseDTO = _cacheService.GetCache().ExchangeResponseDTO
            };

            _cacheService.SetCache(cache);

            return true;
        }

        // Get some transactions
        public async Task<bool> GetTransactionsAsync()
        {
            if (!IsExchanged())
                return false;

            List<Transaction> transactions = _cacheService.GetCache().Transactions;

            if (transactions != null && transactions.Count > 0)
            {
                // The transactions have already been retrieved
                return true;
            }

            AccountsResponseDTO accountsResponseDTO = _cacheService.GetCache().AccountsResponseDTO;

            // If we don't already have the accounts, get them now.
            // (Or if they are no accounts try and get them again.)
            if (accountsResponseDTO == null || accountsResponseDTO.Accounts.Count == 0)
            {
                bool result = await GetAccountsAsync();

                if (!result)
                    return false;
            }

            accountsResponseDTO = _cacheService.GetCache().AccountsResponseDTO;

            if (accountsResponseDTO == null)
                return false;

            IEnumerable<Task<bool>> tasks = accountsResponseDTO.Accounts.Select(i => GetAccountTransactions(i.AccountId, transactions));
            await Task.WhenAll(tasks);

            Cache cache = new Cache
            {
                Transactions = transactions,
                AccountsResponseDTO = _cacheService.GetCache().AccountsResponseDTO,
                ExchangeResponseDTO = _cacheService.GetCache().ExchangeResponseDTO
            };

            _cacheService.SetCache(cache);

            return true;
        }

        // Get the transactions aggregated by category
        public async Task<bool> AggregateAsync()
        {
            if (!IsExchanged())
                return false;

            List<Transaction> transactions = _cacheService.GetCache().Transactions;

            if (transactions == null || transactions.Count == 0)
            {
                // The transactions have not already been retrieved
                bool result = await GetTransactionsAsync();

                if (!result)
                    return false;

                transactions = _cacheService.GetCache().Transactions;
            }

            List<AggregatedTransactions> aggregatedTransactions = transactions
                .Where(t => t.Timestamp > DateTime.Now.AddDays(-7))
                .GroupBy(t => t.TransactionCategory)
                .Select(g => new AggregatedTransactions { TransactionCategory = g.Key, Total = g.Sum(r => r.Amount) })
                .ToList();

            Cache cache = new Cache
            {
                Transactions = transactions,
                AggregatedTransactions = aggregatedTransactions,
                AccountsResponseDTO = _cacheService.GetCache().AccountsResponseDTO,
                ExchangeResponseDTO = _cacheService.GetCache().ExchangeResponseDTO
            };

            _cacheService.SetCache(cache);

            return true;
        }

        // The main genericized request function
        public async Task<IResponse> DoRequest<TResponse>(HttpMethod httpMethod, string uri, bool isAuthRequired = false, IRequest request = null) where TResponse : IResponse
        {
            try
            {
                // apparently don't need a using here: https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
                HttpClient client = _clientFactory.CreateClient();

                if (isAuthRequired)
                {
                    client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _cacheService.GetCache().ExchangeResponseDTO.AccessToken);
                }

                Dictionary<string, string> dict = request.ToDict();

                FormUrlEncodedContent requestContent = new FormUrlEncodedContent(dict);

                HttpResponseMessage response = null;

                if (httpMethod == HttpMethod.Post)
                {
                    response = await client.PostAsync(uri, requestContent);
                }
                else if (httpMethod == HttpMethod.Get)
                {
                    response = await client.GetAsync(uri);
                }

                if (response != null)
                {

                    response.EnsureSuccessStatusCode();
                    string jsonString = await response.Content.ReadAsStringAsync();

                    IResponse responseObj = JsonConvert.DeserializeObject<TResponse>(jsonString);

                    return responseObj;
                }
            }
            catch (Exception ex)
            {
                // TODO: use the logging framework for this
                Console.WriteLine("Error when trying to do api request: " + ex.Message);
            }

            return null;
        }

        // Check whether the code has been exchanged for the access token jwt
        private bool IsExchanged()
        {
            Cache cache = _cacheService.GetCache();

            if (cache == null)
                return false;

            ExchangeResponseDTO exchangeResponseDTO = cache.ExchangeResponseDTO;

            if (exchangeResponseDTO != null && !string.IsNullOrEmpty(exchangeResponseDTO.AccessToken))
                return true;

            return false;
        }

        private async Task<bool> GetAccountTransactions(string accountId, List<Transaction> transactions)
        {
            string uri = _trueLayerCredentials.Value.BaseDataApiUrl + "/data/v1/accounts/" + accountId + "/transactions";

            TransactionsResponseDTO responseObj = (TransactionsResponseDTO)await DoRequest<TransactionsResponseDTO>(HttpMethod.Get, uri, true, null);

            if (responseObj == null)
                return false;

            transactions.AddRange(responseObj.Transactions);

            return true;
        }
    }
}
