using Translator.Infrastructure.Database.Redis.Rudiment;

namespace Translator.Infrastructure.Database.Redis.CacheServices;

public class TemplateCacheService
{
    private readonly IRedisService _redisService;
    private const string KeyPrefix = "global-dict:template:";
    private const string TemplateKeysList = "templates:keys:list";

    public TemplateCacheService(IRedisService redisService)
    {
        _redisService = redisService;
    }

    private static string GetRedisKey(Guid templateId) =>
        $"{KeyPrefix}{templateId}:values";

    public async Task<TemplateCacheDto?> GetTranslationsAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        return await _redisService.GetAsync<TemplateCacheDto>(key);
    }

    public async Task SetTemplateAsync(
        Guid templateId, 
        string templateName, 
        Guid? ownerId,
        string? ownerName,
        List<TranslationDto> translations)
    {
        var key = GetRedisKey(templateId);
        var dto = new TemplateCacheDto(templateId, templateName, ownerId, ownerName, translations);
        
        await _redisService.SetAsync(key, dto);
        
        var keysList = await _redisService.GetAsync<List<string>>(TemplateKeysList) ?? new List<string>();
        if (!keysList.Contains(key))
        {
            keysList.Add(key);
            await _redisService.SetAsync(TemplateKeysList, keysList);
        }
    }

    public async Task DeleteTemplateAsync(Guid templateId)
    {
        var key = GetRedisKey(templateId);
        
        await _redisService.RemoveAsync(key);
        
        var keysList = await _redisService.GetAsync<List<string>>(TemplateKeysList) ?? new List<string>();
        if (keysList.Contains(key))
        {
            keysList.Remove(key);
            await _redisService.SetAsync(TemplateKeysList, keysList);
        }
    }

    public async Task<List<CachedTemplateInfo>> GetCachedTemplatesAsync(int skip = 0, int take = 50)
    {
        var keysList = await _redisService.GetAsync<List<string>>(TemplateKeysList) ?? new List<string>();
        
        var pagedKeys = keysList
            .Skip(skip)
            .Take(take)
            .ToList();
        
        var tasks = pagedKeys
            .Where(key => !string.IsNullOrEmpty(key))
            .Select(async key =>
            {
                var template = await _redisService.GetAsync<TemplateCacheDto>(key);
                
                if (template != null)
                {
                    return new CachedTemplateInfo(
                        template.TemplateId, 
                        template.TemplateName,
                        template.OwnerId,
                        template.OwnerName,
                        template.Translations.Count
                    );
                }
                return null;
            }).ToArray();

        var templateInfos = await Task.WhenAll(tasks);
        
        return templateInfos.Where(t => t != null).ToList()!;
    }

    public async Task<long> GetCachedTemplatesCountAsync()
    {
        var keysList = await _redisService.GetAsync<List<string>>(TemplateKeysList) ?? new List<string>();
        return keysList.Count;
    }
}
public record CachedTemplateInfo(
    Guid TemplateId, 
    string TemplateName, 
    Guid? OwnerId,
    string? OwnerName,
    int ValuesCount);
public record TemplateCacheDto(
    Guid TemplateId, 
    string TemplateName, 
    Guid? OwnerId,
    string? OwnerName,
    List<TranslationDto> Translations);
public record TranslationDto(string Key, string Value, Guid ValueId, string LanguageCode);
