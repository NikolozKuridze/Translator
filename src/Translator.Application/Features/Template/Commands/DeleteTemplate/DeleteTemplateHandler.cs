using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Commands.DeleteTemplate;

public class DeleteTemplateHandler(
    IRepository<TemplateEntity> templateRepository,
    TemplateCacheService templateCacheService)
    : IRequestHandler<DeleteTemplateCommand>
{
    public async Task Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var templateNameHash = TemplateEntity.HashName(request.TemplateName);
        var existsTemplate = await templateRepository
            .Where(t => t.Hash == templateNameHash)
            .SingleOrDefaultAsync(cancellationToken);
        
        if (existsTemplate is null)
            throw new TemplateNotFoundException(request.TemplateName);
        
        await templateRepository.DeleteAsync([existsTemplate]);
        await templateCacheService.DeleteTemplateAsync(existsTemplate.Id);
        await templateRepository.SaveChangesAsync(cancellationToken);
    }
}