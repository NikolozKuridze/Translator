using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Logs.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Logs")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LogsController : Controller
{
    private readonly IMediator _mediator;

    public LogsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("")]
    public async Task<IActionResult> Index(
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        int pageNumber = 1,
        int pageSize = 10,
        int? lastHours = null)
    {
        try
        {
            if (lastHours.HasValue && lastHours.Value > 0)
            {
                dateTo = DateTime.UtcNow;
                dateFrom = DateTime.UtcNow.AddHours(-lastHours.Value);
            }

            var command = new GetLogsCommand(
                new PaginationRequest(
                    page: pageNumber,
                    pageSize: pageSize,
                    dateFrom: dateFrom,
                    dateTo: dateTo,
                    null,
                    null));
            var logs = await _mediator.Send(command);

            var totalCount = logs.TotalItems;
            var totalPages = logs.TotalPages;
            pageNumber = logs.Page;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    data = logs.Items,
                    currentPage = pageNumber,
                    totalPages = totalPages,
                    totalCount = totalCount,
                    pageSize = pageSize,
                    dateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "",
                    dateTo = dateTo?.ToString("yyyy-MM-dd") ?? "",
                    lastHours = lastHours?.ToString() ?? ""
                });
            }

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.LastHours = lastHours?.ToString() ?? "";

            return View(logs.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = $"Error loading logs: {ex.Message}";
            return View(new List<GetLogsResponse>());
        }
    }

    [HttpGet("Details")]
    public async Task<IActionResult> Details(DateTime timestamp)
    {
        try
        {
            var command = new GetLogsCommand(new PaginationRequest(1, 100, null, null, null, null));
            var allLogs = await _mediator.Send(command);

            var targetLog = allLogs.Items.FirstOrDefault(l =>
                Math.Abs((l.Timestamp - timestamp).TotalSeconds) < 1);

            if (targetLog == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Log entry not found." });
                }

                TempData["ErrorMessage"] = "Log entry not found.";
                return RedirectToAction(nameof(Index));
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    data = targetLog,
                    totalCount = allLogs.TotalItems
                });
            }

            ViewBag.LogTimestamp = timestamp;
            ViewBag.TotalCount = allLogs.TotalItems;
            return View(targetLog);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = $"Error loading log details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}