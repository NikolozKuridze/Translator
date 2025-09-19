using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.API.Models;
using Translator.Application.Features.Template.Commands;
using Translator.Application.Features.Template.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[UserAuth]
[ApiController]
[Route("api")]
public class TemplateController : ControllerBase
{
    private readonly IMediator _mediator;

    public TemplateController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create-template")]
    public async Task<IResult> AddTemplate(
        [FromBody] CreateTemplateModel model)
    {
        var command = new CreateTemplate.Command(
            model.TemplateName.Trim(), model.Values);
        await _mediator.Send(command);

        return Results.Ok();
    }

    [HttpGet("search-templates")]
    [ProducesResponseType(typeof(PaginatedResponse<GetAllTemplates.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> GetTemplates(
        string templateName, int pageNumber, int pageSize)
    {
        var command = new SearchTemplate.Command(
            templateName,
            new PaginationRequest(pageNumber, pageSize, null, null, null, null));

        var result = await _mediator.Send(command);

        return Results.Ok(result);
    }

    [HttpGet("get-template/")]
    [ProducesResponseType(typeof(PaginatedResponse<GetTemplate.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> GetTemplate(
        [FromQuery] Guid templateId,
        [FromQuery] bool allTranslates,
        [FromQuery] string? lang = "")
    {
        var command = new GetTemplate.Command(templateId, lang, allTranslates,
            new PaginationRequest(1, 1000, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpGet("get-templates/{pageNumber}/{pageSize}")]
    [ProducesResponseType(typeof(PaginatedResponse<GetAllTemplates.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> GetTemplate(int pageNumber = 1, int pageSize = 10)
    {
        var command = new GetAllTemplates.Command(new PaginationRequest(pageNumber, pageSize, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpDelete("delete-template/{templateName}")]
    public async Task<IResult> DeleteTemplate(
        [FromRoute] string templateName)
    {
        var command = new DeleteTemplate.Command(templateName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
}