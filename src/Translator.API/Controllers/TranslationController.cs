using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.TemplateValue.Commands.CreateTemplateValue;
using Translator.Application.Features.Translation.Commands.CreateTranslation;
using Translator.Application.Features.Translation.Commands.DeleteTranslation;

namespace Translator.API.Controllers;

[ApiController]
public class TranslationController : ControllerBase
{
    private readonly IMediator _mediator;

    public TranslationController(IMediator mediator) => _mediator = mediator;

    [HttpPost("{template}/{templateValue}")]
    public async Task<IResult> AddTemplateValue(
        [FromRoute] string template,
        [FromRoute] string templateValue,
        [FromBody] CreateTranslationContract contract)
    {
        var command = new CreateTranslationCommand(template, templateValue, contract.Value.Trim(), contract.LanguageCode.Trim());        
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("{template}/{templateValue}")]
    public async Task<IResult> DeleteTemplateValue(
        [FromRoute] string template,
        [FromRoute] string templateValue,
        [FromBody] DeleteTranslationContract contract)
    {
        var command = new DeleteTranslationCommand(template, templateValue, contract.Value);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}