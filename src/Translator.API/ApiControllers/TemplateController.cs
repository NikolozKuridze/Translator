using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Application.Features.Template.Queries.GetTemplate;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api")]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("create-template")]
    public async Task<IResult> AddTemplate(
        [FromBody] CreateTemplateModel model)
    {
        var command = new CreateTemplateCommand(
            model.TemplateName.Trim(), model.Values);
        await _mediator.Send(command);
        
        return Results.Ok();
    }
    
    [HttpGet("get-template/")]
    public async Task<IResult> GetTemplate(
        [FromQuery] Guid templateId,
        [FromQuery] bool allTranslates,
        [FromQuery] string? lang = "")
    {
        var command = new GetTemplateCommand(templateId, lang, allTranslates, new PaginationRequest(1, 1000, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpGet("get-templates/{pageNumber}/{pageSize}")]
    public async Task<IResult> GetTemplate(int pageNumber = 1, int pageSize = 10)
    {
        var command = new GetAllTemplatesCommand(new PaginationRequest(pageNumber, pageSize, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpDelete("delete-template/{templateName}")]
    public async Task<IResult> DeleteTemplate(
        [FromRoute] string templateName)
    {
        var command = new DeleteTemplateCommand(templateName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}