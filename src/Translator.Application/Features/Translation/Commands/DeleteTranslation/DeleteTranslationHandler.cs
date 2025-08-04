using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;
using TranslationEntity = Translator.Domain.DataModels.Translation;

namespace Translator.Application.Features.Translation.Commands.DeleteTranslation;

public class DeleteTranslationHandler : IRequestHandler<DeleteTranslationCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public DeleteTranslationHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<Value> templateValueRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _templateRepository = templateRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task Handle(DeleteTranslationCommand request, CancellationToken cancellationToken)
    {
        var valueHash = TemplateEntity.HashName(request.Value);
        var translation = await _translationRepository
            .Where(
                x => x.Value.Hash == valueHash &&
                x.Language.Code == request.LanguageCode && x.IsActive)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (translation is null)
            throw new TranslationNotFoundException(request.LanguageCode);
        
        await _translationRepository.DeleteAsync([translation]);
        await _templateRepository.SaveChangesAsync(cancellationToken);  
    }
}