using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;

using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.DataModels.Template;
using TemplateValueEntity = Translator.Domain.DataModels.TemplateValue;
using TranslationEntity = Translator.Domain.DataModels.Translation;


namespace Translator.Application.Features.TemplateValue.Commands.CreateTemplateValue;

public class CreateTemplateValueHandler : IRequestHandler<CreateTemplateValueCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<TemplateValueEntity> _templateValueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public CreateTemplateValueHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<TemplateValueEntity> templateValueRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _templateRepository = templateRepository;
        _templateValueRepository = templateValueRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task Handle(CreateTemplateValueCommand request, CancellationToken cancellationToken)
    {
        var existsTemplateHash = TemplateEntity.HashName(request.TemplateName);
        var existsTemplateValueHash = TemplateEntity.HashName(request.Key);
        
        var existsTemplate = await _templateRepository
            .Where(t => t.Hash == existsTemplateHash)
            .SingleOrDefaultAsync(cancellationToken);
        
        var existsTemplateValue = await _templateValueRepository
            .Where(t => t.Hash == existsTemplateValueHash)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsTemplateValue is not null)
            throw new TemplateValueAlreadyExistsException(request.Key);
        
        if (existsTemplate == null)
            throw new TemplateNotFoundException(request.TemplateName);

        var templateValue = new TemplateValueEntity(existsTemplate.Id, request.Key);

        var translation = new TranslationEntity(
                templateValue.Id,
                request.Value,
                LanguageDetector.DetectLanguage(request.Value));
        
        await _templateValueRepository.AddAsync(templateValue, cancellationToken);
        await _translationRepository.AddAsync(translation, cancellationToken);
        
        await _templateValueRepository.SaveChangesAsync(cancellationToken);
    }
}