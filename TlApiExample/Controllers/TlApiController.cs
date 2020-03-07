using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
                return BadRequest("Something went wrong when trying to exchange code");
            }

            return Ok("Exchanged Token");
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
