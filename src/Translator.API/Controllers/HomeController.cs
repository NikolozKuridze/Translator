using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Dashboard.Queries;

namespace Translator.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[AdminAuth]
public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var command = new GetDashboardStatistics.Query();
            var statistics = await _mediator.Send(command);
            return View(statistics);
        }
        catch (Exception)
        {
            ViewBag.Error = "Database connection error";
            return View(new GetDashboardStatistics.Response(0, 0, 0, 0, 0, 0, 0, 0, 0));
        }
    }
}