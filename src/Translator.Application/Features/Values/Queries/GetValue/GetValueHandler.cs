using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Values.Queries.GetValue;

public class GetValueHandler : IRequestHandler<GetValueCommand, IEnumerable<GetValueResponse>>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<LanguageEntity> _languageRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IRepository<ValueEntity> _valueRepository;
    private readonly ValueCacheService _valueCacheService;

    public GetValueHandler(
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
    
    public async Task<IEnumerable<GetValueResponse>> Handle(GetValueCommand request, CancellationToken cancellationToken)
    {
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
            if(request.AllTranslations)
                return cachedResult.Translations
                    .Select(t => new GetValueResponse(t.Key, request.ValueId, t.Value, t.LanguageCode));
            
            return cachedResult
                .Translations
                .Where(t => t.LanguageCode == request.LanguageCode)
                .Select(t => new GetValueResponse(t.Key, request.ValueId, t.Value, t.LanguageCode));
        }
        
        if (request.AllTranslations)
        {
            var result = await _valueRepository
                .Where(v => v.Id == request.ValueId)
                .SelectMany(v => v.Translations
                    .Select(t => new GetValueResponse(
                        v.Key, v.Id, t.TranslationValue, t.Language.Code
                    )))
                .ToArrayAsync(cancellationToken);
            
            return result;
        } 
        
        var translation = await _translationRepository
            .Where(
                t => t.Value.Id == request.ValueId && 
                     t.Language.Code == code)
            .Select(t 
                => new GetValueResponse(t.Value.Key, request.ValueId, t.TranslationValue, t.Language.Code))
            .SingleOrDefaultAsync(cancellationToken);
        
        if (translation is null)
            throw new ValueNotFoundException(request.ValueId.ToString());
        
        return [translation];
    }
}

public record GetValueResponse(string ValueKey, Guid Id, string ValueTranslation, string LanguageCode);