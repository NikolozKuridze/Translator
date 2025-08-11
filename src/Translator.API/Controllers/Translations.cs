using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;

namespace Translator.API.Controllers;

[Route("Translations")]
[ApiExplorerSettings(IgnoreApi = true)]
public class Translations : Controller
{
    private readonly IMediator _mediator;

    public Translations(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    { 
        return View(new List<TranslationDto>());
        // TODO: add translations view
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string value, string translation, string languageCode)
    {
        var command = new CreateTranslationCommand(value.Trim(), translation, languageCode.Trim());
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Translation created successfully!";
        return RedirectToAction("Index");
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string value, string languageCode)
    {
        var command = new DeleteTranslationCommand(value, languageCode);
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Translation deleted successfully!";
        return RedirectToAction("Index");
    }
}

public class TranslationDto
{
    public string Value { get; set; }
    public string Translation { get; set; }
    public string LanguageCode { get; set; }
    public string Key { get; set; }
    public bool IsActive { get; set; }
}
