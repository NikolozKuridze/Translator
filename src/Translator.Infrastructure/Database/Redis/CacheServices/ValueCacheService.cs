using System.Text.Json;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class ValueCacheService
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "global-dict:value:";
    private const string ValueKeysList = "value:keys"; // список всех закэшированных value ключей
    private const int Ttl = 21;
    
    public ValueCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string GetRedisKey(Guid valueId) =>
        $"{KeyPrefix}{valueId}:translations";

    public async Task<ValueCacheDto?> GetTranslationsAsync(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        var json = await _db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<ValueCacheDto>(json);
    }

    public async Task SetTranslationsAsync(Guid valueId, string valueKey, List<TranslationDto> translations)
    {
        var key = GetRedisKey(valueId);
        var valueCacheDto = new ValueCacheDto(valueId, valueKey, translations);
        
        var json = JsonSerializer.Serialize(valueCacheDto);
        await _db.StringSetAsync(key, json, TimeSpan.FromDays(Ttl));
        
        await _db.ListRightPushAsync(ValueKeysList, key);
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
    public async Task<List<CachedValueInfo>> GetCachedValuesAsync(int skip = 0, int take = 50)
    {
        var valueKeys = await _db.ListRangeAsync(ValueKeysList, skip, skip + take - 1);
        var result = new List<CachedValueInfo>();

        foreach (var key in valueKeys)
        {
            if (key.IsNullOrEmpty) 
                continue;
            
            var json = await _db.StringGetAsync(key.ToString());
            if (!json.IsNullOrEmpty)
            {
                var value = JsonSerializer.Deserialize<ValueCacheDto>(json);
                if (value != null)
                {
                    result.Add(new CachedValueInfo(
                        value.Id, 
                        value.Key,
                        value.Translations.Count,
                        await GetCachedValuesCountAsync()));
                }
            }
        }
        return result;
    }

    public async Task<long> GetCachedValuesCountAsync()
    {
        return await _db.ListLengthAsync(ValueKeysList);
    }
}

public record CachedValueInfo(Guid ValueId, string ValueKey, int TranslationsCount, long ValuesCount);

public record ValueCacheDto(Guid Id, string Key, List<TranslationDto> Translations);
