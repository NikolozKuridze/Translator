using MediatR;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Value;

public class GetCachedValueHandler : IRequestHandler<GetCachedValueCommand, PaginatedResponse<CachedValueInfo>>
{
    private readonly ValueCacheService _valueCacheService; // 🔧 ИСПРАВЛЕНО: имя переменной

    public GetCachedValueHandler(ValueCacheService valueCacheService)
    {
        _valueCacheService = valueCacheService;
    }
    
    public async Task<PaginatedResponse<CachedValueInfo>> Handle(GetCachedValueCommand request, CancellationToken cancellationToken)
    {
        var skip = (request.Pagination.Page - 1) * request.Pagination.PageSize;
        var take = request.Pagination.PageSize;
        
        var resultsTask = _valueCacheService.GetCachedValuesAsync(skip, take);
        var totalCountTask = _valueCacheService.GetCachedValuesCountAsync();
        
        await Task.WhenAll(resultsTask, totalCountTask);
        
        var result = await resultsTask;
        var totalCount = await totalCountTask;
        
        return new PaginatedResponse<CachedValueInfo>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = (int)totalCount,
            HasNextPage = (long)request.Pagination.Page * request.Pagination.PageSize < totalCount,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = result
        };
    }
}
