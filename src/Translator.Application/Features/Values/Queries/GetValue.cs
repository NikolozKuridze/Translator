using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Values.Queries;

public abstract class GetValue
{
    public sealed record Command(
        Guid ValueId,
        string? LanguageCode,
        bool AllTranslations) : IRequest<IEnumerable<Response>>;  
    
    public sealed record Response(
        string ValueKey,
        Guid Id,
        string ValueTranslation,
        string LanguageCode
    );
    
    public class GetValueHandler : IRequestHandler<Command, IEnumerable<Response>>
{
    private const string DefaultLanguageCode = "en";
    private readonly ICurrentUserService _currentUserService;
    private readonly IRepository<LanguageEntity> _languageRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IRepository<User> _userRepository;
    private readonly ValueCacheService _valueCacheService;
    private readonly IRepository<ValueEntity> _valueRepository;

    public GetValueHandler(
        IRepository<LanguageEntity> languageRepository,
        IRepository<TranslationEntity> translationRepository,
        IRepository<ValueEntity> valueRepository,
        IRepository<User> userRepository,
        ValueCacheService valueCacheService,
        ICurrentUserService currentUserService)
    {
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _valueRepository = valueRepository;
        _userRepository = userRepository;
        _valueCacheService = valueCacheService;
        _currentUserService = currentUserService;
    }

    public async Task<IEnumerable<Response>> Handle(Command request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (!userId.HasValue)
            throw new UnauthorizedAccessException("User authentication required");

        var user = await _userRepository
            .Where(u => u.Id == userId.Value)
            .SingleOrDefaultAsync(cancellationToken);

        if (user == null)
            throw new UserNotFoundException(userId.Value);

        var valueExists = await _valueRepository
            .Where(v => v.Id == request.ValueId &&
                        (v.OwnerId == userId.Value || v.OwnerId == null))
            .SingleOrDefaultAsync(cancellationToken);

        if (valueExists == null)
            throw new ValueNotFoundException($"Value with ID '{request.ValueId}' not found or access denied");

        var code = string.IsNullOrEmpty(request.LanguageCode)
            ? DefaultLanguageCode
            : request.LanguageCode;

        var existsLanguage = await _languageRepository
            .Where(x => x.Code == code)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsLanguage is null)
            throw new LanguageNotFoundException(code);

        var cachedResult = await _valueCacheService.GetTranslationsAsync(request.ValueId);
        if (cachedResult != null)
        {
            if (request.AllTranslations)
                return cachedResult.Translations
                    .Select(t => new Response(t.Key, request.ValueId, t.Value, t.LanguageCode));

            return cachedResult
                .Translations
                .Where(t => t.LanguageCode == request.LanguageCode)
                .Select(t => new Response(t.Key, request.ValueId, t.Value, t.LanguageCode));
        }

        if (request.AllTranslations)
        {
            var result = await _valueRepository
                .Where(v => v.Id == request.ValueId &&
                            (v.OwnerId == userId.Value || v.OwnerId == null))
                .SelectMany(v => v.Translations
                    .Select(t => new Response(
                        v.Key, v.Id, t.TranslationValue, t.Language.Code
                    )))
                .ToArrayAsync(cancellationToken);

            if (!result.Any())
                throw new ValueNotFoundException($"Value with ID '{request.ValueId}' not found or access denied");

            return result;
        }

        var translation = await _translationRepository
            .Where(t => t.Value.Id == request.ValueId &&
                        t.Language.Code == code &&
                        (t.Value.OwnerId == userId.Value || t.Value.OwnerId == null))
            .Select(t => new Response(t.Value.Key, request.ValueId, t.TranslationValue, t.Language.Code))
            .SingleOrDefaultAsync(cancellationToken);

        if (translation is null)
            throw new ValueNotFoundException(
                $"Translation for value '{request.ValueId}' in language '{code}' not found or access denied");

        return [translation];
    }
}

}