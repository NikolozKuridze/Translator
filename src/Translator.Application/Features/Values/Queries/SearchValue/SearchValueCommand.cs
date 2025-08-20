using MediatR;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Domain.Pagination;

namespace Translator.Application.Features.Values.Queries.SearchValue;

public record SearchValueCommand(
    string ValueKey,
    PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<GetAllValuesResponse>>;