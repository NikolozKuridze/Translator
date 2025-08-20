using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Application.Features.Values.Queries.GetValue;
using Translator.Application.Features.Values.Queries.SearchValue;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api")]
public class ValueController : ControllerBase
{
    private readonly IMediator _mediator;

    public ValueController(IMediator mediator) => _mediator = mediator;

    [HttpGet("get-value/")]
    public async Task<IResult> GetValue(
        [FromQuery] Guid valueId,
        [FromQuery] bool allTranslations,
        [FromQuery] string lang = "")
    {
        var command = new GetValueCommand(valueId, lang, allTranslations);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }       

    [HttpGet("get-all-values/")]
    public async Task<IResult> GetValue(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "date", [FromQuery] string sortDirection = "asc")
    {
        var command = new GetAllValuesCommand(
            new PaginationRequest(
                pageNumber,
                pageSize,
                null,
                null,
                null,
                sortBy: sortBy,
                sortDirection: sortDirection));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }

    [HttpGet("search-value")]
    public async Task<IResult> SearchValue(
        [FromQuery] string valueKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var command = new SearchValueCommand(valueKey, new PaginationRequest(page, pageSize, null, null, null, null));
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpPost("add-value")]
    public async Task<IResult> AddValue(
        [FromBody] CreateValueModel model)
    {
        var command = new CreateValueCommand(model.Key.Trim(), model.Value.Trim());
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpDelete("delete-value")]
    public async Task<IResult> DeleteValue(
        [FromBody] DeleteValueModel model)
    {
        var command = new DeleteValueCommand(model.ValueName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
    
}