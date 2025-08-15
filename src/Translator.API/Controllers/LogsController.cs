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
        int pageSize = 10)
    {
        try
        {
            // Конвертируем pageNumber в skip для API
            var skip = (pageNumber - 1) * pageSize;
            var command = new GetLogsCommand(skip, pageSize);
            var logs = (await _mediator.Send(command)).ToList();

            // Фильтрация по дате в памяти (если нужно)
            var filteredLogs = logs.AsEnumerable();
            
            if (dateFrom.HasValue)
            {
                filteredLogs = filteredLogs.Where(l => l.Timestamp.Date >= dateFrom.Value.Date);
            }
            
            if (dateTo.HasValue)
            {
                filteredLogs = filteredLogs.Where(l => l.Timestamp.Date <= dateTo.Value.Date);
            }

            var pagedLogs = filteredLogs.ToList();

            // Получаем общее количество из первого элемента
            var totalCount = logs.FirstOrDefault()?.LogsCount ?? 0;
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd") ?? "";
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd") ?? "";

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
            // Для детального просмотра получаем логи вокруг указанного времени
            var command = new GetLogsCommand(0, 100); // Берем больше для поиска
            var allLogs = await _mediator.Send(command);
            
            // Ищем конкретный лог по timestamp (приблизительно)
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
