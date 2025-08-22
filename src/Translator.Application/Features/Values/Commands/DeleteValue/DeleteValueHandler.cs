using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Values.Commands.DeleteValue;

public class DeleteValueHandler : IRequestHandler<DeleteValueCommand>
{
    private readonly IRepository<Value> _valueRepository;
    private readonly ValueCacheService _valueCacheService;

    public DeleteValueHandler(
        IRepository<Value> valueRepository,
        ValueCacheService valueCacheService,
        TemplateCacheService templateCacheService)
    {
        _valueRepository = valueRepository;
        _valueCacheService = valueCacheService;
    }
    public async Task Handle(DeleteValueCommand request, CancellationToken cancellationToken)
    {
        var valueNameHash = TemplateEntity.HashName(request.ValueName);
        
        var existsValue = await _valueRepository
            .Where(t => t.Hash == valueNameHash) 
            .SingleOrDefaultAsync(cancellationToken);

        if (existsValue is null)
            throw new ValueNotFoundException(request.ValueName);

        await _valueRepository.DeleteAsync([existsValue]);
        await _valueCacheService.DeleteValueTranslationsAsync(existsValue.Id);
        await _valueRepository.SaveChangesAsync(cancellationToken);
    }
}