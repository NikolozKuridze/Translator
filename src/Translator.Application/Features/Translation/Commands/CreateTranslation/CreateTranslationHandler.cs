using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;
using TemplateValueEntity = Translator.Domain.DataModels.TemplateValue;
using TranslationEntity = Translator.Domain.DataModels.Translation;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public class CreateTranslationHandler : IRequestHandler<CreateTranslationCommand, TranslationCreateResponse>
{
    private readonly IRepository<TemplateValueEntity> _templateValueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;
    private readonly IValidator<CreateTranslationCommand> _validator;

    public CreateTranslationHandler(
        IRepository<TemplateValueEntity> templateValueRepository,
        IRepository<TranslationEntity> translationRepository,
        IValidator<CreateTranslationCommand> validator)
    {
        _templateValueRepository = templateValueRepository;
        _translationRepository = translationRepository;
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

        if (templateValue is null)
            throw new TemplateValueNotFoundException(request.TemplateValueName);
        
        if (templateValue.Template is null)
            throw new TemplateNotFoundException(request.TemplateName);
        
        if (request.Language != LanguageDetector.DetectLanguage(request.Value))
            throw new LanguageMissMatchException(request.Value, request.Language.ToString());
        
        if (templateValue.Translations.Any(x => x.Value == request.Value || x.Language == request.Language))
            throw new TranslationAlreadyExistsException(request.Value);
        
        var translation = new TranslationEntity(templateValue.Id, request.Value, request.Language);
        
        await _translationRepository.AddAsync(translation, cancellationToken);
        await _translationRepository.SaveChangesAsync(cancellationToken);
        
        return new TranslationCreateResponse(templateValue.Key, translation.Value);
    }
}

public record TranslationCreateResponse(string Key, string Value);