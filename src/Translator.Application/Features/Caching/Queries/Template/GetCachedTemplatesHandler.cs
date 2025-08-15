using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Template;

public class GetCachedTemplatesHandler : IRequestHandler<GetCachedTemplatesCommand, IEnumerable<GetCachedTemplatesResponse>>
{
    private readonly TemplateCacheService _templateCacheService;

    public GetCachedTemplatesHandler(TemplateCacheService templateCacheService)
    {
        _templateCacheService = templateCacheService;
    }
    
    public async Task<IEnumerable<GetCachedTemplatesResponse>> Handle(GetCachedTemplatesCommand request, CancellationToken cancellationToken)
    {
        var result =
            await _templateCacheService.GetCachedTemplatesAsync(request.Skip, request.Take);

        return result.Select(t => new GetCachedTemplatesResponse(
            t.TemplateName, t.TemplateId, t.ValuesCount, t.TemplatesCount
            ));
    }
}

public record GetCachedTemplatesResponse(string TemplateName, Guid TemplateId, int ValuesCount, long TemplatesCount);