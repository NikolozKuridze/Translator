using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Commands.DeleteLanguage;
using Translator.Application.Features.Language.Queries.GetLanguages;

namespace Translator.API.ApiControllers;

[ApiController]
public class LanguageController : ControllerBase
{
    private readonly IMediator _mediator;

    public LanguageController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("api/language/add")]
    public async Task<IResult> Handle(
        [FromBody] AddLanguageContract request)
    {
        var command = new AddLanguageCommand(request.Code.ToLower().Trim());
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("api/language/remove")]
    public async Task<IResult> Handle(
        [FromBody] DeleteLanguageContract request)
    {
        var command = new DeleteLanguageCommand(request.Code);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpGet("api/languages")]
    public async Task<IResult> Get()
    {
        var command = new GetLanguagesCommand();
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
}