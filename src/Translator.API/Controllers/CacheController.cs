using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Caching.Commands.CacheTemplate;
using Translator.Application.Features.Caching.Commands.CacheValue;
using Translator.Application.Features.Caching.Commands.DeleteTemplateCache;
using Translator.Application.Features.Caching.Commands.DeleteValueCache;
using Translator.Application.Features.Caching.Queries.Template;
using Translator.Application.Features.Caching.Queries.Value;
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

    [HttpGet("")]
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var skip = (pageNumber - 1) * pageSize;
            
            // Получаем закэшированные шаблоны и значения
            var templatesTask = _mediator.Send(new GetCachedTemplatesCommand(skip, pageSize));
            var valuesTask = _mediator.Send(new GetCachedValueCommand(skip, pageSize));
            
            await Task.WhenAll(templatesTask, valuesTask);
            
            var templates = await templatesTask;
            var values = await valuesTask;
            
            // Объединяем в общий список
            var cachedItems = new List<CachedItemViewModel>();
            
            // Добавляем шаблоны
            cachedItems.AddRange(templates.Select(t => new CachedItemViewModel
            {
                Id = t.TemplateId,
                Name = t.TemplateName,
                Type = "Template",
                Count = t.ValuesCount,
                TotalCount = t.TemplatesCount
            }));
            
            // Добавляем значения
            cachedItems.AddRange(values.Select(v => new CachedItemViewModel
            {
                Id = v.ValueId,
                Name = v.ValueKey,
                Type = "Value",
                Count = v.TranslationsCount,
                TotalCount = v.ValuesCount
            }));
            
            // Сортируем по имени
            cachedItems = cachedItems.OrderBy(x => x.Name).ToList();
            
            // Пагинация объединенного списка
            var totalItems = cachedItems.Count;
            var totalPages = totalItems > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));
            
            var pagedItems = cachedItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalItems;
            
            return View(pagedItems);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to load cached items: {ex.Message}";
            return View(new List<CachedItemViewModel>());
        }
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

            var valueCacheDto = new ValueCacheDto(valueId, valueKey, translations);
                
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

    [HttpPost("DeleteTemplate/{templateId:guid}")]
    public async Task<IActionResult> DeleteTemplateCache(Guid templateId)
    {
        try
        {
            var command = new DeleteTemplateCacheCommand(templateId);
            await _mediator.Send(command);
            
            return Json(new { success = true, message = "Template removed from cache successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to remove template from cache: {ex.Message}" });
        }
    }

    [HttpPost("DeleteValue/{valueId:guid}")]
    public async Task<IActionResult> DeleteValueCache(Guid valueId)
    {
        try
        {
            var command = new DeleteCommandCacheCommand(valueId);
            await _mediator.Send(command);
            
            return Json(new { success = true, message = "Value removed from cache successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Failed to remove value from cache: {ex.Message}" });
        }
    }
}

// ViewModel для объединенного списка
public class CachedItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Template" or "Value"
    public int Count { get; set; }
    public long TotalCount { get; set; }
}
