using System.Text.Json;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class TemplateCacheService
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "global-dict:template:";
    private const string TemplateKeysList = "template:keys";
    private const int Ttl = 21;

    public TemplateCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string GetRedisKey(Guid templateId) =>
        $"{KeyPrefix}{templateId}:values";

    public async Task<TemplateCacheDto?> GetTranslationsAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        var json = await _db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return null;

        return JsonSerializer.Deserialize<TemplateCacheDto>(json);
    }

    public async Task SetTemplateAsync(Guid templateId, string templateName, List<TranslationDto> translations)
    {
        var key = GetRedisKey(templateId);
        var dto = new TemplateCacheDto(templateId, templateName, translations);
        var json = JsonSerializer.Serialize(dto);
        
        await _db.StringSetAsync(key, json, TimeSpan.FromDays(Ttl));
        await _db.ListRightPushAsync(TemplateKeysList, key); 
    }
    
    
    public async Task DeleteTemplateAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        await _db.KeyDeleteAsync(key);
    }
    
    public async Task<List<CachedTemplateInfo>> GetCachedTemplatesAsync(int skip = 0, int take = 50)
    {
        var templateKeys = await _db.ListRangeAsync(TemplateKeysList, skip, skip + take - 1);
        var templateCount = await _db.ListLengthAsync(TemplateKeysList);
        
        var result = new List<CachedTemplateInfo>();

        foreach (var key in templateKeys)
        {
            if (key.IsNullOrEmpty) continue;
            
            var json = await _db.StringGetAsync(key.ToString());
            if (!json.IsNullOrEmpty)
            {
                var template = JsonSerializer.Deserialize<TemplateCacheDto>(json);
                
                if (template != null)
                {
                    result.Add(new CachedTemplateInfo(
                        template.TemplateId, 
                        template.TemplateName,
                        template.Translations.Count,
                        await GetCachedTemplatesCountAsync()));
                }
            }
        }

        return result;
    }

    private async Task<long> GetCachedTemplatesCountAsync()
    {
        return await _db.ListLengthAsync(TemplateKeysList);
    }
}

public record CachedTemplateInfo(Guid TemplateId, string TemplateName, int ValuesCount, long TemplatesCount);
public record TemplateCacheDto(Guid TemplateId, string TemplateName, List<TranslationDto> Translations);
public record TranslationDto(string Key, string Value, Guid ValueId, string LanguageCode);