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
                dateFrom = DateTime.UtcNow; 
                dateTo = DateTime.UtcNow.AddHours(lastHours.Value);
            }
        
            var command = new GetLogsCommand(
                new PaginationRequest(
                    pageNumber,
                    pageSize,
                    dateFrom: dateFrom,
                    dateTo: dateFrom ));
            var logs = await _mediator.Send(command);


            var totalCount = logs.TotalItems;
            var totalPages = logs.TotalPages;
            pageNumber = logs.Page;

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
            TempData["ErrorMessage"] = $"Error loading logs: {ex.Message}";
            return View(new List<GetLogsResponse>());
        }
    }

    [HttpGet("Details")]
    public async Task<IActionResult> Details(int logId, DateTime timestamp)
    {
        try
        {
            var command = new GetLogsCommand(new PaginationRequest(0, 100));
            var allLogs = await _mediator.Send(command);
            
            var targetLog = allLogs.Items.FirstOrDefault(l => 
                Math.Abs((l.Timestamp - timestamp).TotalSeconds) < 1);

            if (targetLog == null)
            {
                TempData["ErrorMessage"] = "Log entry not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LogTimestamp = timestamp;
            ViewBag.TotalCount = allLogs.TotalItems;
            return View(targetLog);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading log details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}