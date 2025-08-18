using MediatR;

namespace Translator.Application.Features.Logs.Queries;

public record GetLogsCommand(
    int Skip, 
    int Page,
    DateTimeOffset? DateFrom = null,
    DateTimeOffset? DateTo = null) : IRequest<IEnumerable<GetLogsResponse>>;