using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Values.Commands.CreateValue;
using Translator.Application.Features.Values.Commands.DeleteValue;
using Translator.Application.Features.Values.Queries.GetValue;

namespace Translator.API.Controllers;

[Route("values")]
public class ValueController : ControllerBase
{
    private readonly IMediator _mediator;

    public ValueController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IResult> AddValue(
        [FromBody] CreateValueContract contract)
    {
        var command = new CreateValueCommand(contract.Key.Trim(), contract.Value.Trim());
        await _mediator.Send(command);
        return Results.Ok();
    }

    [HttpDelete("delete")]
    public async Task<IResult> DeleteValue(
        [FromBody] DeleteValueContract contract)
    {
        var command = new DeleteValueCommand(contract.ValueName);
        await _mediator.Send(command);
        return Results.NoContent();
    }

    [HttpPost("get-value")]
    public async Task<IResult> GetValue(
        [FromBody] GetValueContract contract)
    {
        var command = new GetValueCommand(contract.ValueName, contract.LanguageCode);
        var result = await _mediator.Send(command);
        return Results.Ok(result);
    }
}