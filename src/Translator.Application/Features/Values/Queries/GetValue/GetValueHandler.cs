using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.DataModels.Template;
using LanguageEntity = Translator.Domain.DataModels.Language;
using TranslationEntity = Translator.Domain.DataModels.Translation;

namespace Translator.Application.Features.Values.Queries.GetValue;

public class GetValueHandler : IRequestHandler<GetValueCommand, GetValueResponse>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<LanguageEntity> _languageRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public GetValueHandler(
        IRepository<LanguageEntity> languageRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _languageRepository = languageRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task<GetValueResponse> Handle(GetValueCommand request, CancellationToken cancellationToken)
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

        var translation = await _translationRepository
            .Where(
                t => t.Value.Hash == valueHash && 
                     t.Language.Code == code &&
                     t.IsActive)
            .Select(t 
                => new GetValueResponse(t.Value.Key, t.TranslationValue))
            .SingleOrDefaultAsync(cancellationToken);
        
        if (translation is null)
            throw new ValueNotFoundException(request.ValueName);
        
        return translation;
    }
}

public record GetValueResponse(string ValueKey, string ValueTranslation);