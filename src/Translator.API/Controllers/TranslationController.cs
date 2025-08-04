using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;
using Translator.Domain.Contracts;
using Translator.Infrastructure.GoogleService;

namespace Translator.API.Controllers;

[ApiController]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator) 
        => _mediator = mediator;

    
    [HttpPost("create-translation")]
    public async Task<IResult> AddTemplateValue(
        [FromBody] CreateTranslationContract contract)
    {
        var command = new CreateTranslationCommand(contract.Value.Trim(), contract.Translation, contract.LanguageCode.Trim());        
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("delete-translation")]
    public async Task<IResult> DeleteTemplateValue(
        [FromBody] DeleteTranslationContract contract)
    {
        var command = new DeleteTranslationCommand(contract.Value, contract.LanguageCode);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpPost("translate")]
    [ProducesResponseType(typeof(TranslateResponse), 200)]
    public async Task<IActionResult> Translate(
        [FromBody] TranslateRequest request,
        [FromServices] ITranslationService translationService)
    {
        var response = await translationService.TranslateTextAsync(request);
        return Ok(response);
    }
}