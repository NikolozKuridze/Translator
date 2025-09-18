using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Values.Commands.DeleteValue;

public class DeleteValueHandler(
    IRepository<Value> valueRepository,
    IRepository<TemplateEntity> templateRepository,
    ValueCacheService valueCacheService,
    TemplateCacheService templateCacheService)
    : IRequestHandler<DeleteValueCommand>
{
    public async Task Handle(DeleteValueCommand request, CancellationToken cancellationToken)
    {
        var valueNameHash = TemplateEntity.HashName(request.ValueName);

        var existsValue = await valueRepository
            .Where(v => v.Hash == valueNameHash)
            .Include(v => v.Templates)
                .ThenInclude(t => t.Values)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsValue is null)
            throw new ValueNotFoundException(request.ValueName);

        var templatesWithOneValue = existsValue.Templates
            .Where(t => t.Values.Count <= 1)
            .ToArray();
        
        if (templatesWithOneValue.Length != 0)
        {
            await templateRepository.DeleteAsync(templatesWithOneValue);
            foreach (var template in templatesWithOneValue)
                await templateCacheService.DeleteTemplateAsync(template.Id);
        }

        await valueRepository.DeleteAsync([existsValue]);
        await valueCacheService.DeleteValueTranslationsAsync(existsValue.Id);
        await valueRepository.SaveChangesAsync(cancellationToken);
    }
}