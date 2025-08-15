using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.CacheValue;

public class CacheValueHandler : IRequestHandler<CacheValueCommand>
{
    private readonly ValueCacheService _valueCacheService;

    public CacheValueHandler(ValueCacheService valueCacheService)
    {
        _valueCacheService = valueCacheService;
    }
    
    public async Task Handle(CacheValueCommand request, CancellationToken cancellationToken)
    {
        await _valueCacheService.SetTranslationsAsync(
            request.ValueId,
            request.ValueKey,
            request.Translations);
    }
}