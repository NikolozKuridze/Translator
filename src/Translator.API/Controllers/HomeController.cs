using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Dashboard.Queries;

namespace Translator.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var command = new GetDashboardStatisticCommand();
        var statistics = await _mediator.Send(command);
        return View(statistics);
    }
}