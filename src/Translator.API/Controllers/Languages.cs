using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.Language.Commands.DeleteLanguage;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Languages")]
[ApiExplorerSettings(IgnoreApi = true)]
public class Languages : Controller
{
    private readonly IMediator _mediator;

    public Languages(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(
        string filterName = "",
        bool? filterActive = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var query = new GetLanguagesCommand();
            var allLanguages = (await _mediator.Send(query)).ToList();

            var filteredLanguages = allLanguages.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                filteredLanguages = filteredLanguages
                    .Where(l => l.LanguageCode.Contains(filterName, StringComparison.OrdinalIgnoreCase) ||
                                (!string.IsNullOrEmpty(l.LanguageName) &&
                                 l.LanguageName.Contains(filterName, StringComparison.OrdinalIgnoreCase)));
            }

            if (filterActive.HasValue)
            {
                filteredLanguages = filteredLanguages
                    .Where(l => l.IsActive == filterActive.Value);
            }

            filteredLanguages = filteredLanguages
                .OrderByDescending(l => l.IsActive)
                .ThenBy(l => l.LanguageCode);

            var totalCount = filteredLanguages.Count();
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var pagedLanguages = filteredLanguages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    data = pagedLanguages,
                    currentPage = pageNumber,
                    totalPages = totalPages,
                    totalCount = totalCount,
                    pageSize = pageSize,
                    filterName = filterName ?? "",
                    filterActive = filterActive
                });
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.FilterName = filterName ?? "";
            ViewBag.FilterActive = filterActive;

            return View(pagedLanguages);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = $"Error loading languages: {ex.Message}";
            return View(new List<GetLanguagesResponse>());
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] AddLanguage request)
    {
        await _mediator.Send(request);

        TempData["SuccessMessage"] = "Language added successfully!";

        return RedirectToAction("Index");
    }

    [HttpPost("Deactivate")]
    public async Task<IActionResult> Deactivate(string code)
    {
        try
        {
            var command = new DeleteLanguageCommand(code);
            await _mediator.Send(command);

            var successMsg = "Language deactivated successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMsg });
            }

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deactivating language: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Activate")]
    public async Task<IActionResult> Activate(string code)
    {
        try
        {
            var command = new AddLanguage.AddLanguageCommand(code.ToLower().Trim());
            await _mediator.Send(command);

            var successMsg = "Language activated successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMsg });
            }

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error activating language: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("Index");
    }
}