using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.API.Contracts;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;
using Translator.Domain.Contracts;
using Translator.Infrastructure.GoogleService;

namespace Translator.API.ApiControllers;

[ApiController]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator) 
        => _mediator = mediator;
    
    [HttpPost("api/create-translation")]
    public async Task<IResult> AddTranslation(
        [FromBody] CreateTranslationContract contract)
    {
        var command = new CreateTranslationCommand(contract.Value.Trim(), contract.Translation, contract.LanguageCode.Trim());        
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("api/delete-translation")]
    public async Task<IResult> DeleteTranslation(
        [FromBody] DeleteTranslationContract contract)
    {
        var command = new DeleteTranslationCommand(contract.Value, contract.LanguageCode);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpPost("api/translate")]
    [ProducesResponseType(typeof(TranslateResponse), 200)]
    public async Task<IActionResult> Translate(
        [FromBody] TranslateRequest request,
        [FromServices] ITranslationService translationService)
    {
        var response = await translationService.TranslateTextAsync(request);
        return Ok(response);
    }
}