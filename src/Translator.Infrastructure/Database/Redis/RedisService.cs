using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using Translator.Infrastructure.Database.Redis;

namespace TranslationService.Services.Caching
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _cacheDb;
        private readonly RedisConfiguration _configuration;
        private readonly TimeSpan _defaultExpiration;

        public RedisService(
            IConnectionMultiplexer redis, 
            IOptions<RedisConfiguration> configuration)
        {
            _cacheDb = redis.GetDatabase();
            _configuration = configuration.Value;
            _defaultExpiration = TimeSpan.FromSeconds(_configuration.DefaultCacheExpirationMinutes);
        }

        public async Task<T> GetAsync<T>(string key)
        { 
            var value = await _cacheDb.StringGetAsync(key);
           
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            await _cacheDb.StringSetAsync(key, jsonValue, _defaultExpiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
    }
}