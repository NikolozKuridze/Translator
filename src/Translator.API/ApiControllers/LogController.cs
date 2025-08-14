using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Logs.Queries;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/logs")]

public class LogController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet]
    public async Task<IEnumerable<GetLogsResponse>> Handle(
        [FromQuery] int skip = 1, 
        [FromQuery] int page = 10)
    {
        var request  = new GetLogsCommand(skip, page);
        return await _mediator.Send(request);
    }
}