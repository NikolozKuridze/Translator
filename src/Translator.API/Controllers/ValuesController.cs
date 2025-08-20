using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetValue;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("Values")]
public class ValuesController : Controller
{
    private readonly IMediator _mediator;

    public ValuesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string sortBy = "date",
        string sortDirection = "asc",
        int pageNumber = 1,
        int pageSize = 10)
    {   try
        {
            var paginationRequest = new PaginationRequest(pageNumber, pageSize, sortBy: sortBy, sortDirection: sortDirection);

            var command = new GetAllValuesCommand(paginationRequest);
            var result = await _mediator.Send(command);

            ViewBag.CurrentPage = result.Page;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalItems;
            ViewBag.HasNextPage = result.HasNextPage;
            ViewBag.HasPreviousPage = result.HasPreviousPage;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            return View(result.Items);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading values: {ex.Message}";
            return View(new List<GetAllValuesResponse>());
        }
    }
    
    [HttpGet("Details/{valueId:guid}")]
    public async Task<IActionResult> Details(Guid valueId, string? lang)
    {
        if (valueId == Guid.Empty) 
            return RedirectToAction(nameof(Index));
        
        var command = new GetValueCommand(valueId, lang?.Trim(), true);
        var result = await _mediator.Send(command);
        
        var languagesQuery = new GetLanguagesCommand();
        var availableLanguages = (await _mediator.Send(languagesQuery))
            .Where(l => l.IsActive)
            .OrderBy(l => l.LanguageCode)
            .ToList();

        ViewBag.ValueId = valueId;
        ViewBag.CurrentLanguage = lang ?? "";
        ViewBag.AvailableLanguages = availableLanguages;
        
        return View(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string key, string value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                TempData["ErrorMessage"] = "Key and value are required.";
                return RedirectToAction(nameof(Index));
            }

            await _mediator.Send(new CreateValueCommand(key.Trim(), value.Trim()));
            TempData["SuccessMessage"] = "Value created successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating value: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string valueName)
    {
        if (string.IsNullOrWhiteSpace(valueName))
        {
            TempData["ErrorMessage"] = "Value name is required for deletion.";
            return RedirectToAction(nameof(Index));
        }

        var command = new DeleteValueCommand(valueName.Trim());
        await _mediator.Send(command);
        TempData["SuccessMessage"] = "Value deleted successfully!";
        
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("CreateTranslation")]
    public async Task<IActionResult> CreateTranslation(string value, Guid ValueId, string translation, string languageCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(translation) || string.IsNullOrWhiteSpace(languageCode))
            {
                TempData["ErrorMessage"] = "All fields are required for creating translation.";
                return RedirectToAction("Details", new { valueId = ValueId });
            }

            var command = new CreateTranslationCommand(value.Trim(), translation.Trim(), languageCode.Trim());
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Translation created successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating translation: {ex.Message}";
        }

        return RedirectToAction("Details", new { valueId = ValueId });
    }

    [HttpPost("DeleteTranslation")]
    public async Task<IActionResult> DeleteTranslation(string value, Guid ValueId, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(languageCode))
        {
            TempData["ErrorMessage"] = "Value and language code are required for deleting translation.";
            return RedirectToAction("Details", new { valueId = ValueId });
        }

        var command = new DeleteTranslationCommand(value.Trim(), languageCode.Trim());
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Translation deleted successfully!";
        
        return RedirectToAction("Details", new { valueId = ValueId });
    }
}
