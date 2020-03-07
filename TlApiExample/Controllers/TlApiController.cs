using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TlApiExample.Services;

namespace TlApiExample.Controllers
{
    public class TlApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public TlApiController(IUserService userService)
        {
            _userService = userService;
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
    }
}
