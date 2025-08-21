using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Logs.Queries;
using Translator.Application.Features.Logs.Queries.GetLogs;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/logs")]

public class LogController : ControllerBase
{
    private readonly IMediator _mediator;

    public LogController(IMediator mediator)
        => _mediator = mediator;

    [HttpGet]
    public async Task<PaginatedResponse<GetLogsResponse>> Handle(
        [FromQuery] int skip = 1, 
        [FromQuery] int page = 10)
    {
        var request = new GetLogsCommand(new PaginationRequest(skip, page,  null, null, null, null));
        return await _mediator.Send(new GetLogsCommand(new PaginationRequest(skip, page, null, null, null, null)));
    }
}