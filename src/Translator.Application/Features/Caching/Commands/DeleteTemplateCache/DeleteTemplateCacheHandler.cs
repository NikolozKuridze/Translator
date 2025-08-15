using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.DeleteTemplateCache;

public class DeleteTemplateCacheHandler : IRequestHandler<DeleteTemplateCacheCommand>
{
    private readonly TemplateCacheService _templateCacheService;

    public DeleteTemplateCacheHandler(TemplateCacheService templateCacheService)
    {
        _templateCacheService = templateCacheService;
    }
    
    public async Task Handle(DeleteTemplateCacheCommand request, CancellationToken cancellationToken)
    {
        await _templateCacheService.DeleteTemplateAsync(request.TemplateId);
    }
}