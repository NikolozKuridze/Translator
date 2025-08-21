using MediatR;
using Translator.Application.Features.Logs.Queries.GetLogs;

namespace Translator.Application.Features.Logs.Queries.GetLogById;

public record GetLogByIdCommand(long Id) : IRequest<GetLogsResponse?>;