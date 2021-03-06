﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TlApiExample.Entities;
using TlApiExample.Models;
using TlApiExample.Models.Responses;
using TlApiExample.Services;

namespace TlApiExample.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class TlApiController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IHttpRequestService _httpRequestService;

        public TlApiController(
            IUserService userService,
            IHttpRequestService httpRequestService
        )
        {
            _userService = userService;
            _httpRequestService = httpRequestService;
        }

        // TODO: TO BE REMOVED WHEN LOG IN HAS BEEN IMPLEMENTED FULLY IN WEB APP, THIS IS FOR CONVENIENCE ONLY 
        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<IActionResult> LoginConvenience(string username, string password)
        {
            User user = await _userService.AuthenticateAsync(username, password);

            if (user == null)
                return BadRequest(new { message = "Sorry, wrong credentials" });

            return Ok(JsonConvert.SerializeObject(user, new JsonSerializerSettings
                { NullValueHandling = NullValueHandling.Ignore }) );
        }

        [HttpGet("callback")]
        public async Task<IActionResult> CallbackAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest(new { message = "Url param 'code' not provided" });

            bool result = await _httpRequestService.DoExchangeAsync(code);

            if (!result)
                return BadRequest(new { message = "Error trying to exchange code" });

            return Ok(new { message = "OK" });
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> TransactionsAsync()
        {
            List<Transaction> results = await _httpRequestService.GetTransactionsAsync();

            if (results == null)
                return BadRequest(new { message = "Error trying to get transactions" });

            return Ok(JsonConvert.SerializeObject(results));
        }

        [HttpGet("aggregate")]
        public async Task<IActionResult> AggregateAsync()
        {
            List<AggregatedTransaction> results = await _httpRequestService.AggregateAsync();

            if (results == null)
                return BadRequest(new { message = "Error trying to aggregate transactions" });

            return Ok(JsonConvert.SerializeObject(results));
        }
    }
}
