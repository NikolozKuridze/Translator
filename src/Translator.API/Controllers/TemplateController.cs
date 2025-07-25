using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetTemplate;

namespace Translator.API.Controllers;

[ApiController]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("{templateName}/create")]
    public async Task<IResult> AddTemplate(
        [FromRoute] string templateName)
    {
        var command = new CreateTemplateCommand(templateName.Trim());
        await _mediator.Send(command);
        return Results.Ok();
    }
    
    [HttpGet("{templateName}/{lang}")]
    public async Task<IResult> GetTemplate(
        [FromRoute] string templateName,
        [AsParameters] string lang)
    {
        var command = new GetTemplateCommand(templateName, lang);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpDelete("{templateName}")]
    public async Task<IResult> DeleteTemplate(
        [FromRoute] string templateName)
    {
        var command = new DeleteTemplateCommand(templateName);
        await _mediator.Send(command);
        return Results.NoContent();
    }

}