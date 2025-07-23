using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.TemplateValue.Commands.CreateTemplateValue;
using Translator.Application.Features.TemplateValue.Commands.DeleteTemplateValue;

namespace Translator.API.Controllers;

[Route("api/templates/{template}/values")]
public class TemplateValueController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateValueController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IResult> AddTemplateValue(
        [FromRoute] string template,
        [FromBody] CreateTemplateValueContract contract)
    {
        var command = new CreateTemplateValueCommand(template, contract.Key, contract.Value);
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpDelete("{templateValueName}")]
    public async Task<IResult> DeleteTemplateValue(
        [FromRoute] string template,
        [FromRoute] string templateValueName)
    {
        var command = new DeleteTemplateValueCommand(template, templateValueName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}