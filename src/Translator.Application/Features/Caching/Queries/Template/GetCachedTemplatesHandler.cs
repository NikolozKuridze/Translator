using MediatR;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Template;

public class GetCachedTemplatesHandler : IRequestHandler<GetCachedTemplatesCommand, PaginatedResponse<CachedTemplateInfo>>
{
    private readonly TemplateCacheService _templateCacheService;

    public GetCachedTemplatesHandler(TemplateCacheService templateCacheService)
    {
        _templateCacheService = templateCacheService;
    }
    
    public async Task<PaginatedResponse<CachedTemplateInfo>> Handle(GetCachedTemplatesCommand request, CancellationToken cancellationToken)
    {
        var skip = (request.Pagination.Page - 1) * request.Pagination.PageSize;
        var take = request.Pagination.PageSize;
        
        var result = await _templateCacheService.GetCachedTemplatesAsync(skip, take);
        var totalCount = await _templateCacheService.GetCachedTemplatesCountAsync();
        
        return new PaginatedResponse<CachedTemplateInfo>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = (int)totalCount,
            HasNextPage = request.Pagination.Page * request.Pagination.PageSize < totalCount,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = result
        };
    }
}