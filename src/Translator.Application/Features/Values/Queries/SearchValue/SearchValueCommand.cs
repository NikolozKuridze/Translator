using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Values.Queries.SearchValue;

public record SearchValueCommand(string ValueKey, PaginationRequest paginationRequest) : IRequest<PaginatedResponse<SearchValueResponse>>;