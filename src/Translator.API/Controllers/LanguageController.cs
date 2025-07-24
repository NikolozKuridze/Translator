using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Commands.DeleteLanguage;
using Translator.Application.Features.Language.Queries.GetLanguages;

namespace Translator.API.Controllers;

[ApiController]
public class LanguageController : ControllerBase
{
    private readonly IMediator _mediator;

    public LanguageController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("language/create")]
    public async Task<IResult> Handle(
        [FromBody] AddLanguageContract request)
    {
        var command = new AddLanguageCommand(request.Name, request.Code, request.UnicodeRange);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("language/delete")]
    public async Task<IResult> Handle(
        [FromBody] DeleteLanguageContract request)
    {
        var command = new DeleteLanguageCommand(request.Code);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpGet("languages")]
    public async Task<IResult> Get()
    {
        var command = new GetLanguagesCommand();
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
}