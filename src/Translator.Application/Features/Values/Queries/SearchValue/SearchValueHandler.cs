using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.DataModels;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Values.Queries.SearchValue;

public class SearchValueHandler : IRequestHandler<SearchValueCommand, PaginatedResponse<SearchValueResponse>>
{
    private readonly IRepository<Value> _valueRepository;

    public SearchValueHandler(IRepository<Value> valueRepository)
        => _valueRepository = valueRepository;
    
    public async Task<PaginatedResponse<SearchValueResponse>> Handle(SearchValueCommand request, CancellationToken cancellationToken)
    {
        var totalValuesCount = await _valueRepository.AsQueryable().CountAsync(cancellationToken);
        
        var values = _valueRepository
            .Where(v => EF.Functions.Like(v.Key, $"%{request.ValueKey}%"))
            .Skip(request.paginationRequest.Page)
            .Take(request.paginationRequest.PageSize)
            .Select(vk => new SearchValueResponse(
                    vk.Key, vk.Id, vk.Translations.Count, vk.CreatedAt 
                ));
        
        return new PaginatedResponse<SearchValueResponse>()
        {
            Page = request.paginationRequest.Page,
            PageSize = request.paginationRequest.PageSize,
            TotalItems = totalValuesCount,
            HasNextPage = request.paginationRequest.Page * request.paginationRequest.PageSize < totalValuesCount,
            HasPreviousPage = request.paginationRequest.Page > 1,
            Items = await values.ToArrayAsync(cancellationToken)
        };
    }
}
public record SearchValueResponse(string ValueKey, Guid Id, int TranslationsCount, DateTimeOffset CreatedAt);