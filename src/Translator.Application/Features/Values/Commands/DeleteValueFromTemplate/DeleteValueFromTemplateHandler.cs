using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;
namespace Translator.Application.Features.Values.Commands.DeleteValueFromTemplate;

public class DeleteValueFromTemplateHandler : IRequestHandler<DeleteValueFromTemplateCommand>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<Value> _valueRepository;
    private readonly TemplateCacheService _templateCacheService;

    public DeleteValueFromTemplateHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<Value> valueRepository,
        TemplateCacheService templateCacheService)
    {
        _templateRepository = templateRepository;
        _valueRepository = valueRepository;
        _templateCacheService = templateCacheService;
    }
    public async Task Handle(DeleteValueFromTemplateCommand request, CancellationToken cancellationToken)
    {
        var valueHash = TemplateEntity.HashName(request.ValueName);
        
        
        var existsTemplate = await _templateRepository
            .AsQueryable()
            .Include(t => t.Values)
                .ThenInclude(v => v.Translations)
                    .ThenInclude(tr => tr.Language)            .Where(t => t.Id == request.TemplateId)
            .SingleOrDefaultAsync(cancellationToken);

        if (existsTemplate is null)
            throw new TemplateNotFoundException(request.TemplateId.ToString());

        var existsValue = existsTemplate
            .Values
            .SingleOrDefault(x => x.Hash == valueHash);
        
        if (existsValue is null)
            throw new ValueNotFoundException(request.ValueName);
        
        existsTemplate.RemoveValue(existsValue);
        
        var actualTranslations = existsTemplate.Values
            .SelectMany(v => v.Translations
                .Select(t => new TranslationDto(
                    v.Key, t.TranslationValue, v.Id, t.Language.Code
                )))
            .ToList();
        
        await _templateCacheService.DeleteTemplateAsync(request.TemplateId);
        await _templateCacheService.SetTemplateAsync(existsTemplate.Id, existsTemplate.Name, actualTranslations);
        await _templateRepository.SaveChangesAsync(cancellationToken);
    }
}