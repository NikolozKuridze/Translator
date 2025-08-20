using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Commands.DeleteLanguage;
using Translator.Application.Features.Language.Queries.GetLanguages;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api")]
public class LanguageController : ControllerBase
{
    private readonly IMediator _mediator;

    public LanguageController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost("language/add")]
    public async Task<IResult> Handle(
        [FromBody] AddLanguageModel request)
    {
        var command = new AddLanguage.AddLanguageCommand(request.Code.ToLower().Trim());
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("language/remove")]
    public async Task<IResult> Handle(
        [FromBody] DeleteLanguageModel request)
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