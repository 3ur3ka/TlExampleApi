using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using TlApiExample.Entities;
using TlApiExample.Helpers;

namespace TlApiExample.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task Logout();
    }

    public class UserService : IUserService
    {
        // hard coded users for now
        private readonly List<User> _users = new List<User>();

        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            // Create some hard coded users
            CreateUsers();
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {

            if (_users == null)
                return null;

            var user = _users.SingleOrDefault(x => x.Username == username);

            if (user == null)
                return null;

            // verify password
            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // session identity and claims
            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Guid.ToString()));

            await _httpContextAccessor.HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(20)
                }
            );

            return user.RemoveSensitiveFields();
        }

        public async Task Logout()
        {
            await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private void CreatePasswordHashAndSalt(string password, out byte[] hash, out byte[] salt)
        {
            using HMACSHA512 hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using (var hmac = new HMACSHA512(salt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hash[i]) return false;
                }
            }
            return true;
        }

        private void CreateUsers()
        {
            CreatePasswordHashAndSalt("doe", out byte[] hash1, out byte[] salt1);
            CreatePasswordHashAndSalt("doe", out byte[] hash2, out byte[] salt2);
            _users.Add(
                new User
                {
                    Username = "john",
                    PasswordHash = hash1,
                    PasswordSalt = salt1
                }
            );
            _users.Add(
                new User
                {
                    Username = "jane",
                    PasswordHash = hash2,
                    PasswordSalt = salt2
                }
            );
        }
    }
}
