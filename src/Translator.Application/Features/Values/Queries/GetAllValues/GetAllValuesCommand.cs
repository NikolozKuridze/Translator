using MediatR;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Values.Queries.GetAllValues;

public record GetAllValuesCommand(PaginationRequest Pagination) : IRequest<PaginatedResponse<GetAllValuesResponse>>;
