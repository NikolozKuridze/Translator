using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.TemplateValue.Commands.CreateTemplateValue;

namespace Translator.API.Controllers;

[Route("template")]
[ApiController]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IResult> AddTemplate(
        [FromBody] CreateTemplateCommand command)
    {
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpPost("templateValue")]
    public async Task<IResult> AddTemplateValue(
        [FromBody] CreateTemplateValueCommand command)
    {
        await _mediator.Send(command);
        return Results.Ok();
    }
    
    [HttpDelete]
    public async Task<IResult> DeleteTemplate(
        [FromBody] DeleteTemplateCommand command)
    {
        await _mediator.Send(command);
        return Results.Ok();
    }

}