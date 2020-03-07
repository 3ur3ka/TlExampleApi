using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TlApiExample.Helpers;
using TlApiExample.Models;
using TlApiExample.Services;

namespace TlApiExample.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TlApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICacheService _cacheService;
        private readonly IHttpRequestService _httpRequestService;
        private readonly IOptions<TrueLayerCredentials> _trueLayerCredentials;

        public TlApiController(
            IUserService userService,
            IHttpContextAccessor httpContextAccessor,
            ICacheService cacheService,
            IHttpRequestService httpRequestService,
            IOptions<TrueLayerCredentials> trueLayerCredentials)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _cacheService = cacheService;
            _httpRequestService = httpRequestService;
            _trueLayerCredentials = trueLayerCredentials;
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public IActionResult LoginConvenience(string username, string password)
        {
            var user = _userService.AuthenticateAsync(username, password);

            if (user == null)
                return BadRequest(new { message = "Sorry, wrong credentials" });

            return Ok(user);
        }

        [HttpGet("callback")]
        public string Get(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return "You didn't give me a code";
            }

            StoreCode(code);

            return $"Hello {_httpContextAccessor.HttpContext.User.Identity.Name}" +
                $", thanks I got the code! The time is: {DateTime.Now}";
        }

        [HttpGet("exchange")]
        public async Task<IActionResult> ExchangeAsync()
        {
            bool result = await _httpRequestService.DoExchangeAsync();

            if (!result)
            {
                return BadRequest("Error trying to exchange code");
            }

            return Ok("Exchanged Token");
        }

        [HttpGet("accounts")]
        public async Task<IActionResult> AccountsAsync()
        {
            bool result = await _httpRequestService.GetAccountsAsync();

            if (!result)
            {
                return BadRequest("Error trying to get accounts");
            }

            return Ok(JsonConvert.SerializeObject(_cacheService.GetCache().AccountsResponseDTO));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> TransactionsAsync()
        {
            bool result = await _httpRequestService.GetTransactionsAsync();

            if (!result)
            {
                return BadRequest("Error trying to get transactions");
            }

            return Ok($"Transactions: {JsonConvert.SerializeObject(_cacheService.GetCache().Transactions)}");
        }

        [HttpGet("aggregate")]
        public async Task<IActionResult> AggregateAsync()
        {
            bool result = await _httpRequestService.AggregateAsync();

            if (!result)
            {
                return BadRequest("Error trying to aggregate transactions");
            }

            return Ok(JsonConvert.SerializeObject(_cacheService.GetCache().AggregatedTransactions));
        }

        [HttpGet("token")]
        public string Token()
        {
            return _cacheService.GetCache().ExchangeResponseDTO.AccessToken;
        }

        private void StoreCode(string code)
        {
            // Create the cache object and store code in it
            Cache cache = new Cache { Code = code };

            // Set the cache
            _cacheService.SetCache(cache);
        }
    }
}
