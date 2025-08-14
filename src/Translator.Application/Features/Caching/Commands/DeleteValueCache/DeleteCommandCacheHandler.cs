using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Commands.DeleteValueCache;

public class DeleteCommandCacheHandler : IRequestHandler<DeleteCommandCacheCommand>
{
    private readonly ValueCacheService _valueCacheService;

    public DeleteCommandCacheHandler(ValueCacheService valueCacheService)
    {
        _valueCacheService = valueCacheService;
    }
    
    public async Task Handle(DeleteCommandCacheCommand request, CancellationToken cancellationToken)
    {
        await _valueCacheService.DeleteValueTranslationsAsync(request.ValueId);
    }
}