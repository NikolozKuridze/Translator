using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetValue;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;
using Translator.Application.Features.Language.Queries.GetLanguages;

namespace Translator.API.Controllers;

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
    {
        var command = new GetAllValuesCommand(pageNumber, pageSize, sortBy, sortDirection);
        var values = await _mediator.Send(command);

        var totalCount = values.FirstOrDefault()?.TotalCount ?? 0;
        var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
        pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDirection = sortDirection;

        return View(values);
    }

    [HttpGet("Details")]
    public async Task<IActionResult> Details(string valueName, string? lang)
    {
        if (string.IsNullOrWhiteSpace(valueName)) 
            return RedirectToAction(nameof(Index));
        
        var command = new GetValueCommand(valueName.Trim(), lang?.Trim(), true);
        var result = await _mediator.Send(command);
        
        var languagesQuery = new GetLanguagesCommand();
        var availableLanguages = (await _mediator.Send(languagesQuery))
            .Where(l => l.IsActive)
            .OrderBy(l => l.LanguageCode)
            .ToList();

        ViewBag.ValueName = valueName;
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
    public async Task<IActionResult> CreateTranslation(string value, string translation, string languageCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(translation) || string.IsNullOrWhiteSpace(languageCode))
            {
                TempData["ErrorMessage"] = "All fields are required for creating translation.";
                return RedirectToAction("Details", new { valueName = value });
            }

            var command = new CreateTranslationCommand(value.Trim(), translation.Trim(), languageCode.Trim());
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Translation created successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error creating translation: {ex.Message}";
        }

        return RedirectToAction("Details", new { valueName = value });
    }

    [HttpPost("DeleteTranslation")]
    public async Task<IActionResult> DeleteTranslation(string value, string languageCode)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(languageCode))
        {
            TempData["ErrorMessage"] = "Value and language code are required for deleting translation.";
            return RedirectToAction("Details", new { valueName = value });
        }

        var command = new DeleteTranslationCommand(value.Trim(), languageCode.Trim());
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Translation deleted successfully!";
        
        return RedirectToAction("Details", new { valueName = value });
    }
}

