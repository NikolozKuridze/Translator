using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api")]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator) 
        => _mediator = mediator;
    
    [HttpPost("create-translation")]
    public async Task<IResult> AddTranslation(
        [FromBody] CreateTranslationContract contract)
    {
        var command = new CreateTranslationCommand(contract.Value.Trim(), contract.Translation, contract.LanguageCode.Trim());        
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("delete-translation")]
    public async Task<IResult> DeleteTranslation(
        [FromBody] DeleteTranslationContract contract)
    {
        var command = new DeleteTranslationCommand(contract.Value, contract.LanguageCode);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}