using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Logs.Queries;

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
            // Обработка параметра lastHours
            if (lastHours.HasValue && lastHours.Value > 0)
            {
                dateFrom = DateTime.UtcNow.AddHours(-lastHours.Value);
                dateTo = DateTime.UtcNow.AddMinutes(1); 
            }

            var skip = (pageNumber - 1) * pageSize;
        
            // Передаем dateFrom и dateTo в команду
            var command = new GetLogsCommand(skip, pageSize, dateFrom, dateTo);
            var logs = (await _mediator.Send(command)).ToList();

            // Убираем фильтрацию в контроллере, так как она уже происходит в хандлере
            var pagedLogs = logs;

            var totalCount = logs.FirstOrDefault()?.LogsCount ?? 0;
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.LastHours = lastHours?.ToString() ?? "";

            return View(pagedLogs);
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
            var command = new GetLogsCommand(0, 100);
            var allLogs = await _mediator.Send(command);
            
            var targetLog = allLogs.FirstOrDefault(l => 
                Math.Abs((l.Timestamp - timestamp).TotalSeconds) < 1);

            if (targetLog == null)
            {
                TempData["ErrorMessage"] = "Log entry not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.LogTimestamp = timestamp;
            return View(targetLog);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading log details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}