using System.Collections.Immutable;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;
using TemplateValueEntity = Translator.Domain.DataModels.TemplateValue;
using TranslationEntity = Translator.Domain.DataModels.Translation;
using LanguageEntity = Translator.Domain.DataModels.Language;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public class CreateTranslationHandler : IRequestHandler<CreateTranslationCommand, TranslationCreateResponse>
{
    private readonly IRepository<TemplateValueEntity> _templateValueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IRepository<LanguageEntity> _languageEntityRepository;
    private readonly IValidator<CreateTranslationCommand> _validator;

    public CreateTranslationHandler(
        IRepository<TemplateValueEntity> templateValueRepository,
        IRepository<TranslationEntity> translationRepository,
        IRepository<LanguageEntity> languageEntityRepository,
        IValidator<CreateTranslationCommand> validator)
    {
        _templateValueRepository = templateValueRepository;
        _translationRepository = translationRepository;
        _languageEntityRepository = languageEntityRepository;
        _validator = validator;
    }
 
    public async Task<TranslationCreateResponse> Handle(CreateTranslationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var templateValueNameHash = TemplateEntity.HashName(request.TemplateValueName);
        var templateValue = await _templateValueRepository
            .Where(t => t.Hash == templateValueNameHash)
                .Include(x => x.Template)
                .Include(x => x.Translations)
            .SingleOrDefaultAsync(cancellationToken);

        var languages = _languageEntityRepository
            .Where(l => l.IsActive)
            .ToListAsync(cancellationToken).Result;
        
        var textLanguage = LanguageDetector.DetectOrThrow(request.Value, languages);
        
        if (templateValue is null)
            throw new TemplateValueNotFoundException(request.TemplateValueName);
        
        if (templateValue.Template is null)
            throw new TemplateNotFoundException(request.TemplateName);
        
        if (request.LanguageCode != textLanguage.Code)
            throw new LanguageMissMatchException(request.Value, request.LanguageCode);
        
        if (templateValue.Translations.Any(x => x.Value == request.Value || x.Language.Code == request.LanguageCode))
            throw new TranslationAlreadyExistsException(request.Value);
        
        var translation = new TranslationEntity(templateValue.Id, request.Value);
        
        translation.Language = textLanguage;
        
        await _translationRepository.AddAsync(translation, cancellationToken);
        await _translationRepository.SaveChangesAsync(cancellationToken);
        
        return new TranslationCreateResponse(templateValue.Key, translation.Value);
    }
}

public record TranslationCreateResponse(string Key, string Value);