using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Translator.Infrastructure.Database.Redis.Rudiment;


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

    public async Task<string> GetAsync(string key)
    {
        var value = await _cacheDb.StringGetAsync(key);
        
        return !value.IsNullOrEmpty 
            ? string.Empty
            : value.ToString();
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

    // Новые методы для списков
    public async Task ListPushAsync(string key, string value)
    {
        await _cacheDb.ListRightPushAsync(key, value);
    }

    public async Task<string[]> ListRangeAsync(string key, int start = 0, int stop = -1)
    {
        var values = await _cacheDb.ListRangeAsync(key, start, stop);
        return values.Select(v => v.ToString()).ToArray();
    }

    public async Task<long> ListLengthAsync(string key)
    {
        return await _cacheDb.ListLengthAsync(key);
    }

    public async Task ListRemoveAsync(string key, string value)
    {
        await _cacheDb.ListRemoveAsync(key, value);
    }
}