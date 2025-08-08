using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetTemplate;

namespace Translator.API.ApiControllers;

[ApiController]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("api/create-template")]
    public async Task<IResult> AddTemplate(
        [FromBody] CreateTemplateContract contract)
    {
        var command = new CreateTemplateCommand(
            contract.TemplateName.Trim(), contract.Values);
        await _mediator.Send(command);
        
        return Results.Ok();
    }
    
    [HttpGet("api/get-template/{templateName}/{lang?}")]
    public async Task<IResult> GetTemplate(
        [FromRoute] string templateName,
        [FromRoute] string? lang = null)
    {
        var command = new GetTemplateCommand(templateName, lang);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpDelete("api/delete-template/{templateName}")]
    public async Task<IResult> DeleteTemplate(
        [FromRoute] string templateName)
    {
        var command = new DeleteTemplateCommand(templateName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}