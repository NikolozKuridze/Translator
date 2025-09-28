using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.TemplatesAdmin.Commands;
using Translator.Application.Features.TemplatesAdmin.Queries;
using Translator.Application.Features.ValuesAdmin.Commands;
using Translator.Application.Features.ValuesAdmin.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Templates")]
[ApiExplorerSettings(IgnoreApi = true)]
public class TemplatesController : Controller
{
    private readonly IMediator _mediator;

    public TemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string sortBy = "name",
        string sortDirection = "asc",
        int pageNumber = 1,
        int pageSize = 25)
    {
        try
        {
            var command = new AdminGetAllTemplates.Command(
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
                return Json(new
                {
                    success = true,
                    data = templates.Items,
                    currentPage = templates.Page,
                    totalPages = templates.TotalPages,
                    totalCount = templates.TotalItems,
                    pageSize = templates.PageSize,
                    sortBy,
                    sortDirection,
                    globalCount = templates.Items.Count(x => x.IsGlobal),
                    userCount = templates.Items.Count(x => !x.IsGlobal)
                });

            ViewBag.CurrentPage = templates.Page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = templates.TotalPages;
            ViewBag.TotalCount = templates.TotalItems;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            ViewBag.GlobalCount = templates.Items.Count(x => x.IsGlobal);
            ViewBag.UserCount = templates.Items.Count(x => !x.IsGlobal);
            ViewBag.UniqueOwners = templates.Items.Where(x => !x.IsGlobal).Select(x => x.OwnerName).Distinct().Count();

            return View(templates.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = $"Error loading templates: {ex.Message}";
            return View(new List<AdminGetAllTemplates.Response>());
        }
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(
        string? templateName = null,
        string? ownerName = null,
        string? ownershipType = null,
        int pageNumber = 1,
        int pageSize = 25,
        string sortBy = "name",
        string sortDirection = "asc")
    {
        try
        {
            var command = new AdminSearchTemplate.Command(
                templateName,
                ownerName,
                ownershipType,
                new PaginationRequest(pageNumber, pageSize, null, null, null, sortBy, sortDirection));

            var result = await _mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    data = result.Items,
                    currentPage = result.Page,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalItems,
                    pageSize = result.PageSize,
                    sortBy,
                    sortDirection,
                    searchKey = templateName,
                    ownerName,
                    ownershipType,
                    globalCount = result.Items.Count(x => x.IsGlobal),
                    userCount = result.Items.Count(x => !x.IsGlobal)
                });

            ViewBag.TemplateName = templateName;
            ViewBag.OwnerName = ownerName;
            ViewBag.OwnershipType = ownershipType;
            ViewBag.CurrentPage = result.Page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalItems;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.GlobalCount = result.Items.Count(x => x.IsGlobal);
            ViewBag.UserCount = result.Items.Count(x => !x.IsGlobal);

            return View("Index", result.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = $"Error searching templates: {ex.Message}";
            return View("Index", new List<AdminGetAllTemplates.Response>());
        }
    }

    [HttpGet("Details/{templateId:guid}")]
    public async Task<IActionResult> Details(
        Guid templateId,
        string? lang = "en",
        string templateName = "",
        int pageNumber = 1,
        int pageSize = 50)
    {
        try
        {
            if (templateId == Guid.Empty)
                return RedirectToAction(nameof(Index));

            var languagesQuery = new GetLanguagesCommand();
            var availableLanguages = (await _mediator.Send(languagesQuery))
                .Where(l => l.IsActive)
                .OrderBy(l => l.LanguageCode)
                .ToList();

            var query = new AdminGetTemplate.Command(
                templateId,
                lang,
                false,
                new PaginationRequest(pageNumber, pageSize, null, null, null, "key"));

            var allTemplateData = await _mediator.Send(query);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    templateId,
                    templateName,
                    currentLanguage = lang ?? "",
                    availableLanguages,
                    data = allTemplateData.Items,
                    currentPage = allTemplateData.Page,
                    pageSize = allTemplateData.PageSize,
                    totalPages = allTemplateData.TotalPages,
                    totalCount = allTemplateData.TotalItems
                });

            ViewBag.TemplateId = templateId;
            ViewBag.TemplateName = templateName;
            ViewBag.CurrentLanguage = lang ?? "";
            ViewBag.AvailableLanguages = availableLanguages;
            ViewBag.CurrentPage = allTemplateData.Page;
            ViewBag.PageSize = allTemplateData.PageSize;
            ViewBag.TotalPages = allTemplateData.TotalPages;
            ViewBag.TotalCount = allTemplateData.TotalItems;

            return View(allTemplateData.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = $"Error loading template details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string templateName, List<string> values)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName) || !values.Any())
            {
                var errorMsg = "Template name and values are required.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var command = new AdminCreateTemplate.Command(templateName.Trim(), values);
            await _mediator.Send(command);

            var successMsg = "Global template created successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error creating template: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string templateName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                var errorMsg = "Template name is required for deletion.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var command = new AdminDeleteTemplate.Command(templateName.Trim());
            await _mediator.Send(command);

            var successMsg = "Template deleted successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting template: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("Delete/{templateId:guid}")]
    public async Task<IActionResult> DeleteById(Guid templateId)
    {
        try
        {
            var allTemplates =
                await _mediator.Send(
                    new AdminGetAllTemplates.Command(new PaginationRequest(1, 1000, null, null, null, null, null)));
            var templateToDelete = allTemplates.Items.FirstOrDefault(t => t.TemplateId == templateId);

            if (templateToDelete != null)
            {
                var command = new AdminDeleteTemplate.Command(templateToDelete.TemplateName);
                await _mediator.Send(command);
            }

            var successMsg = "Template deleted successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting template: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost("DeleteValueFromTemplate")]
    public async Task<IActionResult> DeleteValueFromTemplate(string valueName, Guid templateId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(valueName))
                return Json(new { success = false, message = "Value name is required." });

            var command = new AdminDeleteValueFromTemplate.Command(valueName.Trim(), templateId);
            await _mediator.Send(command);

            return Json(new { success = true, message = "Value removed from template successfully!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"Error removing value: {ex.Message}" });
        }
    }

    [HttpGet("GetAvailableValues")]
    public async Task<IActionResult> GetAvailableValues()
    {
        try
        {
            var valuesCommand = new AdminGetAllValues.Command(new PaginationRequest(1, 1000, null, null, null, "key"));
            var allValues = await _mediator.Send(valuesCommand);

            var availableValues = allValues.Items.Select(v => new
            {
                key = v.Key,
                valueId = v.ValueId,
                ownershipType = v.OwnershipType,
                ownerName = v.OwnerName,
                translationsCount = v.TranslationsCount
            }).ToList();

            return Json(new { success = true, values = availableValues });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("Statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var allTemplates =
                await _mediator.Send(
                    new AdminGetAllTemplates.Command(new PaginationRequest(1, 10000, null, null, null, null, null)));

            var stats = new
            {
                totalTemplates = allTemplates.TotalItems,
                globalTemplates = allTemplates.Items.Count(x => x.IsGlobal),
                userTemplates = allTemplates.Items.Count(x => !x.IsGlobal),
                uniqueOwners = allTemplates.Items.Where(x => !x.IsGlobal).Select(x => x.OwnerName).Distinct().Count(),
                averageValuesPerTemplate = allTemplates.Items.Any() ? allTemplates.Items.Average(x => x.ValueCount) : 0,
                topOwners = allTemplates.Items
                    .Where(x => !x.IsGlobal)
                    .GroupBy(x => x.OwnerName)
                    .Select(g => new { ownerName = g.Key, templateCount = g.Count() })
                    .OrderByDescending(x => x.templateCount)
                    .Take(5)
                    .ToList()
            };

            return Json(new { success = true, statistics = stats });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}