using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.ValuesAdmin.Queries;

public abstract class AdminGetValue
{
    public sealed record Command(
        Guid ValueId,
        string? LanguageCode,
        bool AllTranslations) : IRequest<IEnumerable<Response>>;  
    
    public sealed record Response(
        string ValueKey,
        Guid Id,
        string ValueTranslation,
        string LanguageCode,
        Guid? OwnerId,
        string OwnerName,
        string OwnershipType
    );
    
    public class AdminGetValueHandler : IRequestHandler<Command, IEnumerable<Response>>
    {
        private const string DefaultLanguageCode = "en";
        private readonly IRepository<LanguageEntity> _languageRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly ValueCacheService _valueCacheService;
        private readonly IRepository<ValueEntity> _valueRepository;

        public AdminGetValueHandler(
            IRepository<LanguageEntity> languageRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<ValueEntity> valueRepository,
            ValueCacheService valueCacheService)
        {
            _languageRepository = languageRepository;
            _translationRepository = translationRepository;
            _valueRepository = valueRepository;
            _valueCacheService = valueCacheService;
        }

        public async Task<IEnumerable<Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var valueExists = await _valueRepository
                .AsQueryable()
                .Include(v => v.Owner)
                .Where(v => v.Id == request.ValueId)
                .SingleOrDefaultAsync(cancellationToken);

            if (valueExists == null)
                throw new ValueNotFoundException($"Value with ID '{request.ValueId}' not found");

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
                var ownerName = valueExists.Owner?.Username ?? "Global";
                var ownershipType = valueExists.OwnerId == null ? "Global" : "User";

                if (request.AllTranslations)
                    return cachedResult.Translations
                        .Select(t => new Response(
                            t.Key, 
                            request.ValueId, 
                            t.Value, 
                            t.LanguageCode,
                            valueExists.OwnerId,
                            ownerName,
                            ownershipType));

                return cachedResult
                    .Translations
                    .Where(t => t.LanguageCode == request.LanguageCode)
                    .Select(t => new Response(
                        t.Key, 
                        request.ValueId, 
                        t.Value, 
                        t.LanguageCode,
                        valueExists.OwnerId,
                        ownerName,
                        ownershipType));
            }

            if (request.AllTranslations)
            {
                var result = await _valueRepository
                    .AsQueryable()
                    .Include(v => v.Owner)
                    .Where(v => v.Id == request.ValueId)
                    .SelectMany(v => v.Translations
                        .Select(t => new Response(
                            v.Key, 
                            v.Id, 
                            t.TranslationValue, 
                            t.Language.Code,
                            v.OwnerId,
                            v.Owner != null ? v.Owner.Username : "Global",
                            v.OwnerId == null ? "Global" : "User"
                        )))
                    .ToArrayAsync(cancellationToken);

                if (!result.Any())
                    throw new ValueNotFoundException($"Value with ID '{request.ValueId}' has no translations");

                return result;
            }

            var translation = await _translationRepository
                .AsQueryable()
                .Include(t => t.Value)
                .ThenInclude(v => v.Owner)
                .Where(t => t.Value.Id == request.ValueId && t.Language.Code == code)
                .Select(t => new Response(
                    t.Value.Key, 
                    request.ValueId, 
                    t.TranslationValue, 
                    t.Language.Code,
                    t.Value.OwnerId,
                    t.Value.Owner != null ? t.Value.Owner.Username : "Global",
                    t.Value.OwnerId == null ? "Global" : "User"))
                .SingleOrDefaultAsync(cancellationToken);

            if (translation is null)
                throw new ValueNotFoundException(
                    $"Translation for value '{request.ValueId}' in language '{code}' not found");

            return [translation];
        }
    }
}
