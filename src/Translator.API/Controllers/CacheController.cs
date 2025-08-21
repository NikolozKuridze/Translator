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
using Translator.Domain.Pagination;
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
    public async Task<IActionResult> Index(
        int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var templatesTask = _mediator.Send(new GetCachedTemplatesCommand(new PaginationRequest(1, int.MaxValue, null, null, null, null)));
            var valuesTask = _mediator.Send(new GetCachedValueCommand(new PaginationRequest(1, int.MaxValue, null, null, null, null)));
            
            await Task.WhenAll(templatesTask, valuesTask);
            
            var templates = await templatesTask;
            var values = await valuesTask;
            
            var cachedItems = new List<CachedItemViewModel>();
            
            cachedItems.AddRange(templates.Items.Select(t => new CachedItemViewModel
            {
                Id = t.TemplateId,
                Name = t.TemplateName,
                Type = "Template",
                Count = t.ValuesCount,
                TotalCount = templates.TotalItems
            }));
            
            cachedItems.AddRange(values.Items.Select(v => new CachedItemViewModel
            {
                Id = v.ValueId,
                Name = v.ValueKey,
                Type = "Value",
                Count = v.TranslationsCount,
                TotalCount = values.TotalItems
            }));
            
            cachedItems = cachedItems.OrderBy(x => x.Name).ToList();
            
            var totalItems = cachedItems.Count;
            var totalPages = totalItems > 0 ? (int)Math.Ceiling((double)totalItems / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));
            
            var pagedItems = cachedItems
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { 
                    success = true, 
                    data = pagedItems,
                    currentPage = pageNumber,
                    totalPages = totalPages,
                    totalCount = totalItems,
                    pageSize = pageSize,
                    templatesCount = templates.Items.Count(),
                    valuesCount = values.Items.Count()        
                });
            }
            
            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalItems;
            ViewBag.TemplatesCount = templates.Items.Count(); 
            ViewBag.ValuesCount = values.Items.Count();       
            
            return View(pagedItems);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }
            
            TempData["ErrorMessage"] = $"Failed to load cached items: {ex.Message}";
            return View(new List<CachedItemViewModel>());
        }
    }


    [HttpPost("CacheTemplate/{templateId:guid}")]
    public async Task<IActionResult> CacheTemplate(Guid templateId)
    {
        try
        {
            var templateQuery = new GetTemplateCommand(
                templateId,
                null,
                true,
                new PaginationRequest(1, 10, null, null, null, null));
            var templateData = await _mediator.Send(templateQuery);

            if (!templateData.Items.Any())
            {
                return Json(new { success = false, message = "Template not found" });
            }

            var templateName = templateData.Items.First().Key; 

            var translations = templateData.Items.Select(t => new TranslationDto(
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

public class CachedItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; 
    public int Count { get; set; }
    public long TotalCount { get; set; }
}
