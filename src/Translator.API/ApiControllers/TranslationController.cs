using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Translation.Commands;
using Translator.Application.Features.Values.Commands;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api")]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create-translation")]
    public async Task<IResult> AddTranslation(
        [FromBody] CreateTranslationModel model)
    {
        var command = new CreateTranslation.Command(model.Value.Trim(), model.Translation, model.LanguageCode.Trim());
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("delete-translation")]
    public async Task<IResult> DeleteTranslation(
        [FromBody] DeleteTranslationModel model)
    {
        var command = new DeleteTranslation.Command(model.Value, model.LanguageCode);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpPatch("update-translation")]
    public async Task<IResult> UpdateTranslation(
        [FromBody] UpdateTranslation.Command model)
    {
        var command = new UpdateTranslation.Command(model.ValueKey, model.LanguageCode,  model.TranslationValue);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpPost("generate-translations-for-values/{languageCode}")]
    public async Task<IResult> GenerateTranslationsForValues(string languageCode)
    {
        var command = new AddTranslationsToAllValues.Command(languageCode);
        var result = await _mediator.Send(command);
        
        return Results.Ok(result);
    }
}