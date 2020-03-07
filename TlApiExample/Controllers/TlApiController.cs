using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
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
        private readonly IDistributedCache _cache;

        public TlApiController(IUserService userService, IHttpContextAccessor httpContextAccessor, IDistributedCache cache)
        {
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
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

            // Store the code against the user Guid
            string key = GetCacheKey();

            // Create the cache object and store code in it
            Cache cache = new Cache { Code = code };

            // Set the cache
            _cache.SetString(key, JsonConvert.SerializeObject(cache));

            return $"Hello {_httpContextAccessor.HttpContext.User.Identity.Name}" +
                $", thanks I got the code! The time is: {DateTime.Now}";
        }

        private string GetCacheKey()
        {
            // Get the user Guid
            return _httpContextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        }

        private Cache GetCache()
        {
            return JsonConvert.DeserializeObject<Cache>(_cache.GetString(GetCacheKey()));
        }
    }
}
