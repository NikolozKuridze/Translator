using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.Entities.Template;
using TranslationEntity = Translator.Domain.Entities.Translation;
using LanguageEntity = Translator.Domain.Entities.Language;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public class CreateTranslationHandler : IRequestHandler<CreateTranslationCommand, TranslationCreateResponse>
{
    private readonly IRepository<Value> _valueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IRepository<LanguageEntity> _languageEntityRepository;
    private readonly IValidator<CreateTranslationCommand> _validator;

    public CreateTranslationHandler(
        IRepository<Value> valueRepository,
        IRepository<TranslationEntity> translationRepository,
        IRepository<LanguageEntity> languageEntityRepository,
        IValidator<CreateTranslationCommand> validator)
    {
        _valueRepository = valueRepository;
        _translationRepository = translationRepository;
        _languageEntityRepository = languageEntityRepository;
        _validator = validator;
    }
 
    public async Task<TranslationCreateResponse> Handle(CreateTranslationCommand request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        
        var valueNameHash = TemplateEntity.HashName(request.ValueName);
        
        var value = await _valueRepository
            .Where(t => t.Hash == valueNameHash)
                .Include(x => x.Translations)
            .SingleOrDefaultAsync(cancellationToken);

        var languages = _languageEntityRepository
            .Where(l => l.IsActive)
            .ToListAsync(cancellationToken).Result;
        
        var textLanguage = LanguageDetector.DetectOrThrow(request.Translation, languages);
        
        if (value is null)
            throw new ValueNotFoundException(request.ValueName);
        
        if (request.LanguageCode != textLanguage.Code)
            throw new LanguageMissMatchException(request.ValueName, request.LanguageCode);
        
        if (value.Translations.Any(
                x => x.TranslationValue == request.Translation ||
                     x.Language.Code == request.LanguageCode))
            throw new TranslationAlreadyExistsException(request.ValueName);
        
        var translation = new TranslationEntity(value.Id, request.Translation)
        {
            Language = textLanguage
        };

        await _translationRepository.AddAsync(translation, cancellationToken);
        await _translationRepository.SaveChangesAsync(cancellationToken);
        
        return new TranslationCreateResponse(value.Key, translation.TranslationValue);
    }
}

public record TranslationCreateResponse(string Key, string Value);