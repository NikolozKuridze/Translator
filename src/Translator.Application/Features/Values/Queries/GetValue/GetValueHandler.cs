using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;
using LanguageEntity = Translator.Domain.DataModels.Language;
using TranslationEntity = Translator.Domain.DataModels.Translation;
using ValueEntity = Translator.Domain.DataModels.Value;

namespace Translator.Application.Features.Values.Queries.GetValue;

public class GetValueHandler : IRequestHandler<GetValueCommand, IEnumerable<GetValueResponse>>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<LanguageEntity> _languageRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IRepository<ValueEntity> _valueRepository;

    public GetValueHandler(
        IRepository<LanguageEntity> languageRepository,
        IRepository<TranslationEntity> translationRepository,
        IRepository<ValueEntity> valueRepository)
    {
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
        _valueRepository = valueRepository;
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
        
        var valueHash = TemplateEntity.HashName(request.ValueName);

        if (request.AllTranslations)
        {
            var result = await _valueRepository
                .Where(v => v.Hash == valueHash)
                .SelectMany(v => v.Translations
                    .Select(t => new GetValueResponse(
                        t.Value.Key, t.TranslationValue, t.Language.Code
                    )))
                .ToArrayAsync(cancellationToken);
            
            return result;
        } 
        
        var translation = await _translationRepository
            .Where(
                t => t.Value.Hash == valueHash && 
                     t.Language.Code == code)
            .Select(t 
                => new GetValueResponse(t.Value.Key, t.TranslationValue, t.Language.Code))
            .SingleOrDefaultAsync(cancellationToken);
        
        if (translation is null)
            throw new ValueNotFoundException(request.ValueName);
        
        return [translation];
    }
}

public record GetValueResponse(string ValueKey, string ValueTranslation, string LanguageCode);