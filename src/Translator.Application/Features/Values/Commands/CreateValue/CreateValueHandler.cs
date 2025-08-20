using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;
using TranslationEntity = Translator.Domain.Entities.Translation;
using LanguageEntity = Translator.Domain.Entities.Language;


namespace Translator.Application.Features.Values.Commands.CreateValue;

public class CreateValueHandler : IRequestHandler<CreateValueCommand>
{
    private readonly IRepository<Value> _templateValueRepository;
    private readonly IRepository<LanguageEntity> _languageEntityRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public CreateValueHandler(
        IRepository<Value> templateValueRepository,
        IRepository<LanguageEntity> languageEntityRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _templateValueRepository = templateValueRepository;
        _languageEntityRepository = languageEntityRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task Handle(CreateValueCommand request, CancellationToken cancellationToken)
    { 
        var existsTemplateValueHash = TemplateEntity.HashName(request.Key);
        
        var existsValue = await _templateValueRepository
            .Where(t => t.Hash == existsTemplateValueHash)
            .SingleOrDefaultAsync(cancellationToken);

        var languages = await _languageEntityRepository
            .Where(l => l.IsActive)
            .ToListAsync(cancellationToken);
        
        var textLanguage = LanguageDetector.DetectOrThrow(request.Value, languages);
        
        if (existsValue is not null)
            throw new ValueAlreadyExistsException(request.Key);
        
        var value = new Value(request.Key);

        var translation = new TranslationEntity(
                value.Id,
                request.Value);
        translation.Language = textLanguage;
        
        await _templateValueRepository.AddAsync(value, cancellationToken);
        await _translationRepository.AddAsync(translation, cancellationToken);
        
        await _templateValueRepository.SaveChangesAsync(cancellationToken);
    }
}