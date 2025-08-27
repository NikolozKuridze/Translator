using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Features.Values.Queries.GetAllValues;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Values.Queries.SearchValue;

public class SearchValueHandler : IRequestHandler<SearchValueCommand, PaginatedResponse<GetAllValuesResponse>>
{
    private readonly IRepository<Value> _valueRepository;

    public SearchValueHandler(IRepository<Value> valueRepository)
        => _valueRepository = valueRepository;
    
    public async Task<PaginatedResponse<GetAllValuesResponse>> Handle(SearchValueCommand request, CancellationToken cancellationToken)
    {
        var query = _valueRepository
            .Where(v =>
                string.IsNullOrEmpty(request.ValueKey) ||
                v.Key.ToLower().Contains(request.ValueKey.ToLower()) ||
                v.Key.ToLower() == request.ValueKey.ToLower());

        var totalItems = await query.CountAsync(cancellationToken);

        var values = await query
            .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
            .Take(request.PaginationRequest.PageSize)
            .Select(vk => new GetAllValuesResponse(
                vk.Key, vk.Id, vk.Translations.Count, vk.CreatedAt
            ))
            .ToArrayAsync(cancellationToken);

        return new PaginatedResponse<GetAllValuesResponse>()
        {
            Page = request.PaginationRequest.Page,
            PageSize = request.PaginationRequest.PageSize,
            TotalItems = totalItems,
            HasNextPage = request.PaginationRequest.Page * request.PaginationRequest.PageSize < totalItems,
            HasPreviousPage = request.PaginationRequest.Page > 1,
            Items = values
        };
    }
}