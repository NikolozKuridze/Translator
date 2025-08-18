using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.Rudiment
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _cacheDb;
        private readonly TimeSpan _defaultExpiration;

        public RedisService(
            IConnectionMultiplexer redis, 
            IOptions<RedisConfiguration> configuration)
        {
            _cacheDb = redis.GetDatabase();
            var configurationValue = configuration.Value;
            _defaultExpiration = TimeSpan.FromSeconds(configurationValue.DefaultCacheExpirationMinutes);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var value = await _cacheDb.StringGetAsync(key);
            
            if (!value.HasValue)
                return default;
            
            return JsonSerializer.Deserialize<T>(value);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            var jsonValue = JsonSerializer.Serialize(value);
            var isTrue = await _cacheDb.StringSetAsync(key, jsonValue, _defaultExpiration);
        }

        public async Task RemoveAsync(string key)
        {
            await _cacheDb.KeyDeleteAsync(key);
        }
    }
}