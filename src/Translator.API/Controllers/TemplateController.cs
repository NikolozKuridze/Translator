using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Template.Commands.CreateTemplate;

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

}