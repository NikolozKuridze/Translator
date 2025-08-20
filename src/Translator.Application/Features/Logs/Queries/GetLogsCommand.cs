using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Logs.Queries;

public record GetLogsCommand(PaginationRequest Pagination) : IRequest<PaginatedResponse<GetLogsResponse>>;