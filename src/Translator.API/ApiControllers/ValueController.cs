using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.API.Models;
using Translator.Application.Features.Values.Commands;
using Translator.Application.Features.Values.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[UserAuth]
[ApiController]
[Route("api")]
public class ValueController : ControllerBase
{
    private readonly IMediator _mediator;

    public ValueController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-value/")]
    [ProducesResponseType(typeof(IEnumerable<GetValue.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> GetValue(
        [FromQuery] Guid valueId,
        [FromQuery] bool allTranslations,
        [FromQuery] string lang = "")
    {
        var command = new GetValue.Command(valueId, lang, allTranslations);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpGet("get-all-values/")]
    [ProducesResponseType(typeof(PaginatedResponse<GetAllValues.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> GetAllValues(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "date",
        [FromQuery] string sortDirection = "asc")
    {
        var command = new GetAllValues.Command(
            new PaginationRequest(
                pageNumber,
                pageSize,
                null,
                null,
                null,
                sortBy,
                sortDirection));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpGet("search-value")]
    [ProducesResponseType(typeof(PaginatedResponse<GetAllValues.Response>), StatusCodes.Status200OK)]
    public async Task<IResult> SearchValue(
        [FromQuery] string valueKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var command = new SearchValue.Command(valueKey, new PaginationRequest(page, pageSize, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpPost("add-value")]
    [ProducesResponseType(typeof(CreateValue.Response), StatusCodes.Status201Created)]
    public async Task<IResult> AddValue(
        [FromBody] CreateValueModel model)
    {
        var command = new CreateValue.Command(model.Key.Trim(), model.Value.Trim());
        var result = await _mediator.Send(command);
        
        return Results.Created("create-value", result);
    }

    [HttpDelete("delete-value")]
    public async Task<IResult> DeleteValue([FromBody] DeleteValueModel model)
    {
        var command = new DeleteValue.Command(model.ValueName);
        await _mediator.Send(command);
        return Results.Ok(new { success = true, message = "Value deleted successfully" });
    }
}