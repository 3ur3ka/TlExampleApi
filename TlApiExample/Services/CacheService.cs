﻿using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using TlApiExample.Models;

namespace TlApiExample.Services
{
    public interface ICacheService
    {
        Cache GetCache();
        void SetCache(Cache cache);
    }

    public class CacheService : ICacheService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IDistributedCache _cache;

        public CacheService(IHttpContextAccessor httpContextAccessor,
            IDistributedCache cache)
        {
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public Cache GetCache()
        {
            string cacheStr = _cache.GetString(GetCacheKey());

            if (string.IsNullOrEmpty(cacheStr))
                return null;

            return JsonConvert.DeserializeObject<Cache>(cacheStr);
        }

        public void SetCache(Cache cache)
        {
            _cache.SetString(GetCacheKey(), JsonConvert.SerializeObject(cache));
        }

        private string GetCacheKey()
        {
            // Get the logged in user Guid
            return _httpContextAccessor.HttpContext.User.Claims
                .SingleOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
        }
    }
}
