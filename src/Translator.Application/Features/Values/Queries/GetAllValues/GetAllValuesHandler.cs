using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Values.Queries.GetAllValues;

public class GetAllValuesHandler : IRequestHandler<GetAllValuesCommand, PaginatedResponse<GetAllValuesResponse>>
{
    private readonly IRepository<ValueEntity> _valueRepository;

    public GetAllValuesHandler(IRepository<ValueEntity> valueRepository)
    {
        _valueRepository = valueRepository;
    }
    
    public async Task<PaginatedResponse<GetAllValuesResponse>> Handle(GetAllValuesCommand request, CancellationToken cancellationToken)
    {
        var query = _valueRepository
            .AsQueryable()
            .AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var sortBy = request.Pagination?.SortBy?.ToLower();
        var sortDirection = request.Pagination?.SortDirection?.ToLower();

        query = sortBy switch
        {
            "date" => sortDirection == "desc"
                ? query.OrderByDescending(v => v.CreatedAt)
                : query.OrderBy(v => v.CreatedAt),
            
            "key" => sortDirection == "desc" 
                ? query.OrderByDescending(v => v.Key)
                : query.OrderBy(v => v.Key),
            _ => query
        };

        var items = await query
            .Skip((request.Pagination!.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .Select(t => new GetAllValuesResponse(
                t.Key, 
                t.Id,
                t.Translations.Count, 
                t.CreatedAt))
            .ToArrayAsync(cancellationToken);

        return new PaginatedResponse<GetAllValuesResponse>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = totalCount,
            HasNextPage = (request.Pagination.Page * request.Pagination.PageSize) < totalCount,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = items
        };
    }
}

public record GetAllValuesResponse(string Key, Guid ValueId, int TranslationsCount, DateTimeOffset CreatedAt);
