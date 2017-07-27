using System;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using BoxCLI.BoxPlatform.Cache;
using BoxCLI.BoxPlatform.Utilities;
using BoxCLI.BoxHome;

namespace BoxCLI.BoxPlatform.Cache
{
    public class BoxPlatformCache : IBoxPlatformCache
    {
        public const string CACHE_PREFIX = "box_cli";
        public const string ENTERPRISE = "enterprise";
        public const string EXPIRES_AT = "expires_at";
        public const string CACHE_DELIMITER = "/";

        private static readonly TimeSpan TOKEN_EXPIRATION_PERIOD = TimeSpan.FromMinutes(45);
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly IMemoryCache _memoryCache;
        private readonly IBoxHome BoxHome;

        public BoxPlatformCache(IMemoryCache memoryCache, IBoxHome boxHome)
        {
            _memoryCache = memoryCache;
            BoxHome = boxHome;
        }

        public BoxCachedToken BustCache()
        {
            var key = ConstructCacheKey();
            _memoryCache.Remove(key);
            return BoxHome.BustCache();
        }

        public BoxCachedToken GetToken(Func<string> generateToken)
        {
            string tokenString;
            var token = new BoxCachedToken();

            var tokenKey = ConstructCacheKey();

            //Check in-memory cache for token first...
            _memoryCache.TryGetValue(tokenKey, out tokenString);
            if (!string.IsNullOrEmpty(tokenString))
            {
                //If something was found in memory, deserialize it...
                token = DeserializeToken(tokenString);
            }
            else
            {
                //Check PersistantCache next...
                token = BoxHome.GetBoxCache().RetrieveTokenFromCache();
            }

            //If nothing was found in either cache...
            if (string.IsNullOrEmpty(token.AccessToken))
            {
                token = HandleExpiredOrEmptyToken(token, tokenKey, generateToken);
                SetTokenInMemory(tokenKey, JsonConvert.SerializeObject(token));
            }
            else
            {
                //Check if the token from cache is expired...
                if (IsTokenExpired(token))
                {
                    token = HandleExpiredOrEmptyToken(token, tokenKey, generateToken);
                    SetTokenInMemory(tokenKey, JsonConvert.SerializeObject(token));
                }
            }
            return token;
        }

        private BoxCachedToken DeserializeToken(string token)
        {
            return JsonConvert.DeserializeObject<BoxCachedToken>(token);
        }

        private BoxCachedToken HandleExpiredOrEmptyToken(BoxCachedToken token, string tokenKey, Func<string> generateToken)
        {
            token.AccessToken = null;
            token.ExpiresAt = null;
            token.AccessToken = generateToken();
            token = AddExpirationToToken(token);
            var tokenString = JsonConvert.SerializeObject(token);
            SetTokenInMemory(tokenKey, tokenString);
            BoxHome.GetBoxCache().SetTokenInCache(token);
            return token;
        }

        private void SetTokenInMemory(string tokenKey, string token)
        {
            this._memoryCache.Set(tokenKey, token, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TOKEN_EXPIRATION_PERIOD));
        }

        private BoxCachedToken AddExpirationToToken(BoxCachedToken token)
        {
            if (token.ExpiresAt == null)
            {
                var expirationTime = DateTime.UtcNow.Add(TOKEN_EXPIRATION_PERIOD);
                var timestamp = ToUnixTimestamp(expirationTime);
                token.ExpiresAt = timestamp.ToString();
            }
            return token;
        }

        private bool IsTokenExpired(BoxCachedToken token)
        {
            if (token.ExpiresAt == null)
            {
                return true;
            }

            var expirationTime = ToDateTimeFromUnixTimestamp(token.ExpiresAt);
            if (DateTime.Now.CompareTo(expirationTime) > -1)
            {
                return true;
            }
            return false;
        }

        private static long ToUnixTimestamp(DateTime input)
        {
            var time = input.Subtract(new TimeSpan(EPOCH.Ticks));
            return (long)(time.Ticks / 10000);
        }

        private static DateTime ToDateTimeFromUnixTimestamp(string timestamp)
        {
            return EPOCH.AddMilliseconds(Convert.ToInt64(timestamp)).ToLocalTime();
        }

        private string ConstructCacheKey()
        {
            return CACHE_PREFIX + CACHE_DELIMITER + ENTERPRISE;
        }
    }
}