using MediatR;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Value;

public class GetCachedValueHandler : IRequestHandler<GetCachedValueCommand, IEnumerable<GetCachedValueResponse>>
{
    private readonly ValueCacheService _valueCacheDto;

    public GetCachedValueHandler(ValueCacheService valueCacheDto)
    {
        _valueCacheDto = valueCacheDto;
    }
    
    public async Task<IEnumerable<GetCachedValueResponse>> Handle(GetCachedValueCommand request, CancellationToken cancellationToken)
    {
        var result = await _valueCacheDto.GetCachedValuesAsync(request.Skip, request.Take);
        return result
            .Select(t => new GetCachedValueResponse(
                    t.ValueId, t.ValueKey, t.TranslationsCount, t.ValuesCount
                ));
    }
}

public record GetCachedValueResponse(Guid ValueId, string ValueKey, int TranslationsCount, long ValuesCount);