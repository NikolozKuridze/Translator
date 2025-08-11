using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Commands.DeleteLanguage;

namespace Translator.API.Controllers;

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
    public async Task<IActionResult> Index()
    {
        var query = new GetLanguagesCommand();
        var languages = await _mediator.Send(query);
        return View(languages);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string code)
    {
        var command = new AddLanguageCommand(code.ToLower().Trim());
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Language added successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost("Deactivate")]
    public async Task<IActionResult> Deactivate(string code)
    {
        var command = new DeleteLanguageCommand(code);
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Language deactivated successfully!";
        return RedirectToAction("Index");
    }
}
