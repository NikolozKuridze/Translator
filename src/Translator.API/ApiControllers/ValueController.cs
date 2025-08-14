using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Application.Features.Values.Queries.GetValue;

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
        var command = new GetAllValuesCommand(pageNumber, pageSize, sortBy, sortDirection);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
    
    [HttpPost("add-value")]
    public async Task<IResult> AddValue(
        [FromBody] CreateValueContract contract)
    {
        var command = new CreateValueCommand(contract.Key.Trim(), contract.Value.Trim());
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpDelete("delete-value")]
    public async Task<IResult> DeleteValue(
        [FromBody] DeleteValueContract contract)
    {
        var command = new DeleteValueCommand(contract.ValueName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
    
}