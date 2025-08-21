using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Logs.Queries.GetLogs;

public record GetLogsCommand(PaginationRequest Pagination, int? Level = 3)
    : IRequest<PaginatedResponse<GetLogsResponse>>;