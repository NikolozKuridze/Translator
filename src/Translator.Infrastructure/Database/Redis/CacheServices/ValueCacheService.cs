using Translator.Infrastructure.Database.Redis.Rudiment;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class ValueCacheService
{
    private readonly IRedisService _redisService;
    private const string KeyPrefix = "global-dict:value:";
    private const string ValueKeysList = "values:keys:list";
    
    public ValueCacheService(IRedisService redisService)
    {
        _redisService = redisService;
    }

    private static string GetRedisKey(Guid valueId) =>
        $"{KeyPrefix}{valueId}:translations";

    public async Task<ValueCacheDto?> GetTranslationsAsync(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        return await _redisService.GetAsync<ValueCacheDto>(key);
    }

    public async Task<bool> IsValueCached(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        var value = await _redisService.GetAsync<ValueCacheDto>(key);
        return value != null;
    }

    public async Task SetTranslationsAsync(Guid valueId, string valueKey, List<TranslationDto> translations)
    {
        var key = GetRedisKey(valueId);
        var valueCacheDto = new ValueCacheDto(valueId, valueKey, translations);
        
        
        await _redisService.SetAsync(key, valueCacheDto);
        
        var keysList = await _redisService.GetAsync<List<string>>(ValueKeysList) ?? new List<string>();
        if (!keysList.Contains(key))
        {
            keysList.Add(key);
            await _redisService.SetAsync(ValueKeysList, keysList);
        }
    }

    public async Task<List<TranslationDto>> GetValueByLanguageAsync(Guid valueId, string languageCode)
    {
        var values = await GetTranslationsAsync(valueId);
        return values?.Translations
            .Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            .ToList() ?? new List<TranslationDto>();
    }

    public async Task DeleteValueTranslationsAsync(Guid valueId)
    {
        var key = GetRedisKey(valueId);
        
        await _redisService.RemoveAsync(key);
        
        var keysList = await _redisService.GetAsync<List<string>>(ValueKeysList) ?? new List<string>();
        if (keysList.Contains(key))
        {
            keysList.Remove(key);
            await _redisService.SetAsync(ValueKeysList, keysList);
        }
    }

    public async Task<List<CachedValueInfo>> GetCachedValuesAsync(int skip = 0, int take = 50)
    {
        var keysList = await _redisService.GetAsync<List<string>>(ValueKeysList) ?? new List<string>();
        
        var pagedKeys = keysList.Skip(skip).Take(take).ToList();
        
        var tasks = pagedKeys
            .Where(key => !string.IsNullOrEmpty(key))
            .Select(async key =>
            {
                try
                {
                    var value = await _redisService.GetAsync<ValueCacheDto>(key);
                    
                    if (value != null)
                    {
                        return new CachedValueInfo(
                            value.Id, 
                            value.Key,
                            value.Translations.Count
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting value for key {key}: {ex.Message}");
                }
                return null;
            }).ToArray();

        var valueInfos = await Task.WhenAll(tasks);
        
        return valueInfos.Where(v => v != null).ToList()!;
    }

    public async Task<long> GetCachedValuesCountAsync()
    {
        var keysList = await _redisService.GetAsync<List<string>>(ValueKeysList) ?? new List<string>();
        return keysList.Count;
    }
}

public record CachedValueInfo(Guid ValueId, string ValueKey, int TranslationsCount);
public record ValueCacheDto(Guid Id, string Key, List<TranslationDto> Translations);
