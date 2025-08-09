using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetValue;

namespace Translator.API.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Route("Values")]
public class ValuesController : Controller
{
    private readonly IMediator _mediator;

    public ValuesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("Search")]
    public async Task<IActionResult> Search([FromQuery] string valueName, [FromQuery] string? lang)
    {
        var result = await _mediator.Send(new GetValueCommand(valueName.Trim(), lang?.Trim()));
        return View("Index", result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromForm]string key, [FromForm]string value)
    {
        await _mediator.Send(new CreateValueCommand(key.Trim(), value.Trim()));
        return RedirectToAction("Index");
    }

    [HttpGet("Delete")]
    public async Task<IActionResult> Delete([FromQuery] string valueName)
    {
        var query = new DeleteValueCommand(valueName.Trim());
        await _mediator.Send(query);
        return RedirectToAction("Index");
    }
}