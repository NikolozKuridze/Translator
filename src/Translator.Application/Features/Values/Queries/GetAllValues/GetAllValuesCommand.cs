using MediatR;

namespace Translator.Application.Features.Values.Queries.GetAllValues;

public record GetAllValuesCommand(int PageNumber = 1, int PageSize = 10,
    string SortBy = "date", string SortDirection = "asc") : IRequest<IEnumerable<GetAllValuesResponse>>;