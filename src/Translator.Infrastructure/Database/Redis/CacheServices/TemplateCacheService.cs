using System.Text.Json;
using StackExchange.Redis;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class TemplateCacheService
{
    private readonly IDatabase _db;
    private const string KeyPrefix = "global-dict:template:";
    private const int Ttl = 21;

    public TemplateCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    private static string GetRedisKey(Guid templateId) =>
        $"{KeyPrefix}{templateId}:values";

    public async Task<TemplateCacheDto> GetTranslationsAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        var json = await _db.StringGetAsync(key);

        if (json.IsNullOrEmpty)
            return default;

        return JsonSerializer.Deserialize<TemplateCacheDto>(json);
    }

    public async Task SetTemplateAsync(Guid templateId, string templateName, List<TranslationDto> translations)
    {
        var key = GetRedisKey(templateId);
        var dto = new TemplateCacheDto(templateId, templateName, translations);
        var json = JsonSerializer.Serialize(dto);
        await _db.StringSetAsync(key, json, TimeSpan.FromDays(Ttl));
    }

    public async Task<List<TranslationDto>> GetValueByLanguageAsync(Guid templateId, string languageCode)
    {
        var template = await GetTranslationsAsync(templateId);
        return template
            .Translations
            .Where(t => t.LanguageCode.Equals(languageCode, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    
    public async Task DeleteTemplateAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        await _db.KeyDeleteAsync(key);
    }
}
public record TemplateCacheDto(Guid TemplateId, string TemplateName, List<TranslationDto> Translations);
public record TranslationDto(string Key, string Value, string LanguageCode);