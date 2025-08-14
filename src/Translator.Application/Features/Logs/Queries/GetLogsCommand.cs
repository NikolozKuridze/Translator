using MediatR;

namespace Translator.Application.Features.Logs.Queries;

public record GetLogsCommand(int Skip = 1, int Page = 10) : IRequest<IEnumerable<GetLogsResponse>>;
