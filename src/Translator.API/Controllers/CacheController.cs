using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Caching.Commands.CacheTemplate;
using Translator.Application.Features.Caching.Commands.CacheValue;
using Translator.Application.Features.Values.Queries.GetValue;
using Translator.Application.Features.Template.Queries.GetTemplate;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.API.Controllers;

[AdminAuth]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("Cache")]
public class CacheController : Controller
{
    private readonly IMediator _mediator;

    public CacheController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("CacheTemplate/{templateId:guid}")]
    public async Task<IActionResult> CacheTemplate(Guid templateId)
    {
        try
        {
            var templateQuery = new GetTemplateCommand(templateId, null, true);
            var templateData = (await _mediator.Send(templateQuery)).ToList();

            if (!templateData.Any())
            {
                return Json(new { success = false, message = "Template not found" });
            }

            var templateName = templateData.First().Key; 

            var translations = templateData.Select(t => new TranslationDto(
                t.Key,
                t.Value,
                t.ValueId,
                t.LanguageCode ?? "en"
            )).ToList();

            var templateCacheDto = new TemplateCacheDto(
                templateId,
                templateName,
                translations
            );
            
            var cacheCommand = new CacheTemplateCommand(
                templateCacheDto.TemplateId,
                templateCacheDto.TemplateName,
                templateCacheDto.Translations
                );
            await _mediator.Send(cacheCommand);

            return Json(new { success = true, message = $"Template '{templateName}' cached successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to cache template: {ex.Message}" });
        }
    }

    [HttpPost("CacheValue/{valueId:guid}")]
    public async Task<IActionResult> CacheValue(Guid valueId)
    {
        try
        {
            var valueQuery = new GetValueCommand(valueId, null, true);
            var valueData = (await _mediator.Send(valueQuery)).ToList();

            if (!valueData.Any())
            {
                return Json(new { success = false, message = "Value not found" });
            }

            var valueKey = valueData.First().ValueKey;
            var translations = valueData.Select(v => new TranslationDto(
                v.ValueKey,
                v.ValueTranslation,
                v.Id,
                v.LanguageCode
            )).ToList();

            var valueCacheDto = new ValueCacheDto
            {
                Id = valueId,
                Key = valueKey,
                Translations = translations
            };

            var cacheCommand = new CacheValueCommand(
                valueCacheDto.Id,
                valueCacheDto.Key,
                valueCacheDto.Translations
                );
            await _mediator.Send(cacheCommand);

            return Json(new { success = true, message = $"Value '{valueKey}' cached successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to cache value: {ex.Message}" });
        }
    }
}
