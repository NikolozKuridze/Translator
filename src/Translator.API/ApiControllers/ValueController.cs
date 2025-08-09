using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetValue;

namespace Translator.API.ApiControllers;

[ApiController]
public class ValueController : ControllerBase
{
    private readonly IMediator _mediator;

    public ValueController(IMediator mediator) => _mediator = mediator;

    [HttpGet("api/get-value/{valueName}/{lang?}")]
    public async Task<IResult> GetValue(
        [FromRoute] string valueName,
        [FromRoute] string? lang = null)
    {
        var command = new GetValueCommand(valueName, lang);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }       
    
    [HttpPost("api/add-value")]
    public async Task<IResult> AddValue(
        [FromBody] CreateValueContract contract)
    {
        var command = new CreateValueCommand(contract.Key.Trim(), contract.Value.Trim());
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpDelete("api/delete-value")]
    public async Task<IResult> DeleteValue(
        [FromBody] DeleteValueContract contract)
    {
        var command = new DeleteValueCommand(contract.ValueName);
        await _mediator.Send(command);
        return Results.NoContent();
    }
    
}