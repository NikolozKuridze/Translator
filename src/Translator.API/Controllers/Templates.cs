using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Application.Features.Template.Queries.GetTemplate;
using Translator.Application.Features.Values.Commands.DeleteValueFromTemplate;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Templates")]
[ApiExplorerSettings(IgnoreApi = true)]
public class Templates : Controller
{
    private readonly IMediator _mediator;

    public Templates(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string sortBy = "name",
        string sortDirection = "asc",
        int pageNumber = 1,
        int pageSize = 10)
    {
        var command = new GetAllTemplatesCommand(
            new PaginationRequest(
                pageNumber,
                pageSize,
                null,
                null,
                null,
                sortBy,
                sortDirection));

        var templates = await _mediator.Send(command);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true,
                data = templates.Items,
                currentPage = templates.Page,
                totalPages = templates.TotalPages,
                totalCount = templates.TotalItems,
                pageSize = pageSize,
                sortBy = sortBy,
                sortDirection = sortDirection
            });
        }

        ViewBag.CurrentPage = templates.Page;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = templates.TotalPages;
        ViewBag.TotalCount = templates.TotalItems;
        ViewBag.SortBy = sortBy;
        ViewBag.SortDirection = sortDirection;

        return View(templates.Items);
    }

    [HttpGet("Details")]
    public async Task<IActionResult> Details(
        Guid templateId, string? lang = "en", string templateName = "",
        int pageNumber = 1, int pageSize = 10)
    {
        if (templateId == Guid.Empty)
            return RedirectToAction(nameof(Index));

        var languagesQuery = new GetLanguagesCommand();
        var availableLanguages = (await _mediator.Send(languagesQuery))
            .Where(l => l.IsActive)
            .OrderBy(l => l.LanguageCode)
            .ToList();

        var query = new GetTemplateCommand(
            templateId,
            lang,
            false,
            new PaginationRequest(pageNumber, pageSize, null, null, null, null));
        var allTemplateData = await _mediator.Send(query);

        var totalCount = allTemplateData.TotalItems;
        var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
        pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

        var pagedTemplateData = allTemplateData
            .Items
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();


        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new
            {
                success = true,
                templateId = templateId,
                templateName = templateName,
                currentLanguage = lang ?? "",
                availableLanguages = availableLanguages,
                data = pagedTemplateData,
                currentPage = pageNumber,
                pageSize = pageSize,
                totalPages = totalPages,
                totalCount = totalCount
            });
        }

        ViewBag.TemplateId = templateId;
        ViewBag.TemplateName = templateName;
        ViewBag.CurrentLanguage = lang ?? "";
        ViewBag.AvailableLanguages = availableLanguages;
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalCount = totalCount;

        return View(pagedTemplateData);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string templateName, List<string> values)
    {
        try
        {
            var command = new CreateTemplateCommand(templateName.Trim(), values);
            await _mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Template created successfully" });
            }

            TempData["SuccessMessage"] = "Template created successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string templateName)
    {
        try
        {
            var command = new DeleteTemplateCommand(templateName.Trim());
            await _mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = "Template deleted successfully" });
            }

            TempData["SuccessMessage"] = "Template deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
    
    [HttpPost("DeleteValueFromTemplate")]
    public async Task<IActionResult> DeleteValueFromTemplate([FromBody] DeleteValueFromTemplateCommand command)
    {
        try
        {
            await _mediator.Send(command);
            return Json(new { success = true, message = "Value deleted successfully" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }     
}