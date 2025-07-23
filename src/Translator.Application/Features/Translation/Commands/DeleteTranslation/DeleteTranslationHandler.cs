using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;
using TemplateValueEntity = Translator.Domain.DataModels.TemplateValue;
using TranslationEntity = Translator.Domain.DataModels.Translation;

namespace Translator.Application.Features.Translation.Commands.DeleteTranslation;

public class DeleteTranslationHandler : IRequestHandler<DeleteTranslationCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<TemplateValueEntity> _templateValueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public DeleteTranslationHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<TemplateValueEntity> templateValueRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _templateRepository = templateRepository;
        _templateValueRepository = templateValueRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task Handle(DeleteTranslationCommand request, CancellationToken cancellationToken)
    {
        var translation = await ValidateIncomeData(
            request,
            _templateRepository, 
            _templateValueRepository,
            _translationRepository,
            cancellationToken);

        await _translationRepository.DeleteAsync([translation]);
        await _templateRepository.SaveChangesAsync(cancellationToken);  
    }

    private static async Task<TranslationEntity> ValidateIncomeData(
        DeleteTranslationCommand command,
        IRepository<TemplateEntity> templateRepository,
        IRepository<TemplateValueEntity> templateValueRepository,
        IRepository<TranslationEntity> translationRepository,
        CancellationToken cancellationToken)
    {
        var templateNameHash = TemplateEntity.HashName(command.Template);
        var templateValueNameHash = TemplateEntity.HashName(command.TemplateValue);
        
        
        var template = await templateRepository
            .Where(t => t.Hash == templateNameHash)
            .SingleOrDefaultAsync(cancellationToken);
        if (template is null)
            throw new TemplateNotFoundException(command.Template);
        
        var templateValue = await templateValueRepository
            .Where(t => t.Hash == templateValueNameHash)
            .SingleOrDefaultAsync(cancellationToken);
        if (templateValue is null)
            throw new TemplateValueNotFoundException(command.TemplateValue);
        
        var translation = await translationRepository
            .Where(x => x.TemplateValueId == templateValue.Id
                        && x.Value == command.Value)
            .SingleOrDefaultAsync(cancellationToken);
        if (translation is null)
            throw new TranslationNotFoundException(templateNameHash);
        
        return translation;
    }
}