using MediatR;
using Microsoft.Extensions.Logging;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheTemplate;

public class CacheTemplateHandler : IRequestHandler<CacheTemplateCommand>
{
    private readonly TemplateCacheService _templateCacheService;

    public CacheTemplateHandler(TemplateCacheService templateCacheService)
    {
        _templateCacheService = templateCacheService;
    }
    
    public async Task Handle(CacheTemplateCommand request, CancellationToken cancellationToken)
    {
        await _templateCacheService.SetTemplateAsync(request.TemplateId, request.TemplateName, request.Values);
    }
}