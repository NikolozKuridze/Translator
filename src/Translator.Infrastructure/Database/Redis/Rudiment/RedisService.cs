using System.Text.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.Rudiment;

public class RedisService : IRedisService
{
    private readonly IDatabase _cacheDb;
    private readonly RedisOptions _configuration;
    private readonly TimeSpan _defaultExpiration;

    public RedisService(
        IConnectionMultiplexer redis, 
        IOptions<RedisOptions> configuration)
    {
        _cacheDb = redis.GetDatabase();
        _configuration = configuration.Value;
        _defaultExpiration = TimeSpan.FromDays(_configuration.DefaultCacheExpirationDays); 
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _cacheDb.StringGetAsync(key);

        if (!value.HasValue)
            return default;
        
        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<string> GetAsync(string key)
    {
        var value = await _cacheDb.StringGetAsync(key);
        
        return value.HasValue 
            ? value.ToString()
            : string.Empty; 
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