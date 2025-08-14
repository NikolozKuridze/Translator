using System.Text.Json;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class ValueCacheService
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "global-dict:value:";
    private const int Ttl = 21;
    
    public ValueCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string GetRedisKey(Guid valueId) =>
        $"{KeyPrefix}{valueId}:translations";

    public async Task<ValueCacheDto> GetTranslationsAsync(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        var json = await _db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<ValueCacheDto>(json);
    }

    public async Task SetTranslationsAsync(Guid valueId, string valueKey, List<TranslationDto> translations)
    {
        var key = GetRedisKey(valueId);
        var valueCacheDto = new ValueCacheDto
        {
            Id = valueId,
            Key = valueKey,
            Translations = translations,
        };
        var json = JsonSerializer.Serialize(valueCacheDto);
        await _db.StringSetAsync(key, json, TimeSpan.FromDays(Ttl));
    }

    public async Task<List<TranslationDto>> GetValueByLanguageAsync(Guid valueId, string languageCode)
    {
        var values = await GetTranslationsAsync(valueId);
        return values
            .Translations
            .Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    

    public async Task DeleteValueTranslationsAsync(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        await _db.KeyDeleteAsync(key);
    }
}

public class ValueCacheDto
{
    public Guid Id { get; set; }
    public string Key { get; set; }
    public List<TranslationDto> Translations { get; set; }
}