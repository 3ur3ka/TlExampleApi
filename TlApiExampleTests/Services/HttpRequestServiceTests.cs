using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TlApiExample.Helpers;
using TlApiExample.Models;
using TlApiExample.Models.Responses;
using TlApiExample.Services;
using Xunit;

namespace TlApiExampleTests.Services
{
    public class HttpRequestServiceTests
    {
        private Mock<IHttpClientFactory> mockHttpClientFactory = new Mock<IHttpClientFactory>();

        // Need to mock the handler here, can't mock HttpClient directly unfortunately as it has unoverridable members
        private HttpClient httpClient;

        private readonly Mock<ICacheService> mockCacheService = new Mock<ICacheService>();

        private HttpRequestService httpRequestService;

        // Dummy TL creds
        private readonly TrueLayerCredentials trueLayerCredentials =
            new TrueLayerCredentials {
                BaseAuthUrl = "https://www.example.com",
                BaseDataApiUrl = "https://www.example.com",
                ClientId = "",
                ClientSecret = "",
                RedirectUrl = ""
            };

        private readonly IOptions<TrueLayerCredentials> trueLayerCredentialsOptions;

        private readonly string dummyCode = "1234";
        private readonly string dummyAccessToken = "5678";
        private readonly string dummyCacheKey = "000c6036-5c68-4f6c-9c98-3c8260de0027";
        private readonly Cache dummyCacheWithCode;
        private readonly Cache dummyCacheWithCodeAndAccessToken;
        private readonly Cache dummyCacheWithAccounts;
        private readonly ExchangeResponseDTO dummyExchangeResponseDTO;
        private readonly AccountsResponseDTO dummyAccountsResponseDTO;
        private readonly TransactionsResponseDTO dummyTransactionsResponseDTO;

        private readonly List<Transaction> dummyTransactions;

        public HttpRequestServiceTests()
        {
            trueLayerCredentialsOptions = Options.Create(trueLayerCredentials);
            dummyCacheWithCode = new Cache { Code = dummyCode };
            dummyExchangeResponseDTO = new ExchangeResponseDTO { AccessToken = dummyAccessToken };

            Account account1 = new Account { AccountId = "1" };
            Account account2 = new Account { AccountId = "2" };

            dummyAccountsResponseDTO = new AccountsResponseDTO
            {
                Accounts = new List<Account> { account1, account2 }
            };
            dummyCacheWithCodeAndAccessToken = new Cache {
                Code = dummyCode,
                ExchangeResponseDTO = dummyExchangeResponseDTO
            };
            dummyCacheWithAccounts = new Cache
            {
                Code = dummyCode,
                ExchangeResponseDTO = dummyExchangeResponseDTO,
                AccountsResponseDTO = dummyAccountsResponseDTO
            };

            Transaction transaction1 = new Transaction
            {
                Amount = -1.0m,
                TransactionCategory = "PURCHASE",
                Timestamp = DateTime.Parse("2020-03-06T00:00:00+00:00")
            };

            Transaction transaction2 = new Transaction
            {
                Amount = 2.0m,
                TransactionCategory = "BILL_PAYMENT",
                Timestamp = DateTime.Parse("2020-04-06T00:00:00+00:00")
            };

            dummyTransactions = new List<Transaction> { transaction1, transaction2 };
            dummyTransactionsResponseDTO = new TransactionsResponseDTO { Transactions = dummyTransactions };
        }

        [Fact]
        public async Task TestDoExchangeAsync()
        {
            // Arrange
            mockCacheService.Setup(_ => _.GetCacheKey()).Returns(dummyCacheKey);
            mockCacheService.Setup(_ => _.GetCache()).Returns(dummyCacheWithCode);

            Cache cacheThatWasSet = null;

            mockCacheService.Setup(_ => _.SetCache(It.IsAny<Cache>()))
                .Callback<Cache>((cache) => cacheThatWasSet = cache);

            SetupHttpClientForExchangePost();
            SetupHttpClientFactory();

            SetupHttpRequestService();

            httpRequestService = new HttpRequestService(mockHttpClientFactory.Object,
                mockCacheService.Object, trueLayerCredentialsOptions);

            // Act
            await httpRequestService.DoExchangeAsync();

            // Assert
            mockCacheService.Verify(_ => _.GetCache());
            Assert.NotNull(cacheThatWasSet.ExchangeResponseDTO);
            Assert.Equal(cacheThatWasSet.ExchangeResponseDTO.AccessToken, dummyAccessToken);
        }

        [Fact]
        public async Task TestGetAccounts()
        {
            // Arrange
            mockCacheService.Setup(_ => _.GetCacheKey()).Returns(dummyCacheKey);
            mockCacheService.Setup(_ => _.GetCache()).Returns(dummyCacheWithCodeAndAccessToken);

            Cache cacheThatWasSet = null;

            mockCacheService.Setup(_ => _.SetCache(It.IsAny<Cache>()))
                .Callback<Cache>((cache) => cacheThatWasSet = cache);

            SetupHttpClientForGetAccounts();
            SetupHttpClientFactory();

            SetupHttpRequestService();

            // Act
            await httpRequestService.GetAccountsAsync();

            // Assert
            Assert.Equal(cacheThatWasSet.AccountsResponseDTO.Accounts[0].AccountId,
                dummyAccountsResponseDTO.Accounts[0].AccountId);

            Assert.Equal(cacheThatWasSet.AccountsResponseDTO.Accounts[1].AccountId,
                dummyAccountsResponseDTO.Accounts[1].AccountId);
        }

        [Fact]
        public async Task TestGetTransactionsWithoutCallingAccountsFirst()
        {
            // Arrange
            mockCacheService.Setup(_ => _.GetCacheKey()).Returns(dummyCacheKey);

            int count = 0;
            mockCacheService.Setup(_ => _.GetCache())
                .Returns(() => count++ <= 3 ? dummyCacheWithCodeAndAccessToken : dummyCacheWithAccounts);

            Cache cacheThatWasSet = null;

            mockCacheService.Setup(_ => _.SetCache(It.IsAny<Cache>()))
                .Callback<Cache>((cache) => cacheThatWasSet = cache);

            SetupHttpClientForGetTransactions();
            SetupHttpClientFactory();

            SetupHttpRequestService();

            // Act
            await httpRequestService.GetTransactionsAsync();

            // Assert
            Assert.True(IsEqual(cacheThatWasSet.Transactions[0], dummyTransactions[0]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[1], dummyTransactions[1]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[2], dummyTransactions[0]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[3], dummyTransactions[1]));
        }

        [Fact]
        public async Task TestGetTransactionsAccountsAlreadyGot()
        {
            // Arrange
            mockCacheService.Setup(_ => _.GetCacheKey()).Returns(dummyCacheKey);

            mockCacheService.Setup(_ => _.GetCache())
                .Returns(dummyCacheWithAccounts);

            Cache cacheThatWasSet = null;

            mockCacheService.Setup(_ => _.SetCache(It.IsAny<Cache>()))
                .Callback<Cache>((cache) => cacheThatWasSet = cache);

            SetupHttpClientForGetTransactions();
            SetupHttpClientFactory();

            SetupHttpRequestService();

            // Act
            await httpRequestService.GetTransactionsAsync();

            // Assert
            Assert.True(IsEqual(cacheThatWasSet.Transactions[0], dummyTransactions[0]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[1], dummyTransactions[1]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[2], dummyTransactions[0]));
            Assert.True(IsEqual(cacheThatWasSet.Transactions[3], dummyTransactions[1]));
        }

        private void SetupHttpClientFactory()
        {
            mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);
        }

        private void SetupHttpClientForExchangePost()
        {
            // Setup dummy token in response
            HttpContent content = new StringContent($@"{{ access_token: ""{dummyAccessToken}"" }}");
            HttpResponseMessage message = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };

            SetupHttpMessageHandler(message);
        }

        private void SetupHttpClientForGetAccounts()
        {
            // Setup dummy response from httpclient
            SetupHttpMessageHandler(GetAccountsResponseMessage());
        }

        private HttpResponseMessage GetAccountsResponseMessage()
        {
            HttpContent content = new StringContent(JsonConvert.SerializeObject(dummyAccountsResponseDTO));
            return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };
        }

        private void SetupHttpClientForGetTransactions()
        {
            // Setup dummy response from httpclient
            HttpContent content = new StringContent(JsonConvert.SerializeObject(dummyTransactionsResponseDTO));
            HttpResponseMessage message = new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = content };

            SetupHttpMessageHandler(message);
        }

        private void SetupHttpMessageHandler(HttpResponseMessage message)
        {
            Mock<HttpMessageHandler> mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(() => Task.FromResult(message));

            httpClient = new HttpClient(mockHttpMessageHandler.Object);
        }

        private void SetupHttpRequestService()
        {
            httpRequestService = new HttpRequestService(mockHttpClientFactory.Object,
                mockCacheService.Object, trueLayerCredentialsOptions);
        }

        private bool IsEqual(Transaction transaction1, Transaction transaction2)
        {
            return transaction1.Amount == transaction2.Amount &&
                transaction1.Timestamp == transaction2.Timestamp &&
                transaction1.TransactionCategory == transaction2.TransactionCategory;
        }
    }
}
