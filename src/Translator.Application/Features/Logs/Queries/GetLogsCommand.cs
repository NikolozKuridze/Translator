using MediatR;

namespace Translator.Application.Features.Logs.Queries;

public sealed record GetLogsCommand(
    int Skip, 
    int PageSize,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null) : IRequest<IEnumerable<GetLogsResponse>>;