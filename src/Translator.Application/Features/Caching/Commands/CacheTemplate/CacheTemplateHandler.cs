using MediatR;
using Microsoft.Extensions.Logging;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheTemplate;

public class CacheTemplateHandler(TemplateCacheService templateCacheService) : IRequestHandler<CacheTemplateCommand>
{
    public async Task Handle(CacheTemplateCommand request, CancellationToken cancellationToken)
    {
        await templateCacheService.SetTemplateAsync(
            request.TemplateId,
            request.TemplateName,
            request.OwnerId,
            request.OwnerName,
            request.Values);
    }
}