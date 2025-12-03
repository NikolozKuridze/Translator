using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.ValuesAdmin.Commands;

public abstract class AdminDeleteValueFromTemplate
{
    public sealed record Command(
        string ValueName, 
        Guid TemplateId) : IRequest;
    
    public class AdminDeleteValueFromTemplateHandler : IRequestHandler<Command>
    {
        private readonly TemplateCacheService _templateCacheService;
        private readonly IRepository<TemplateEntity> _templateRepository;

        public AdminDeleteValueFromTemplateHandler(
            IRepository<TemplateEntity> templateRepository,
            IRepository<Value> valueRepository,
            TemplateCacheService templateCacheService)
        {
            _templateRepository = templateRepository;
            _templateCacheService = templateCacheService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var valueHash = TemplateEntity.HashName(request.ValueName);

            var existsTemplate = await _templateRepository
                .AsQueryable()
                .Include(t => t.Values)
                .ThenInclude(v => v.Translations)
                .ThenInclude(tr => tr.Language)
                .Include(t => t.Owner)
                .Where(t => t.Id == request.TemplateId)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is null)
                throw new TemplateNotFoundException($"Template with ID '{request.TemplateId}' not found");

            var existsValue = existsTemplate
                .Values
                .SingleOrDefault(x => x.Hash == valueHash);

            if (existsValue is null)
            {
                var templateOwner = existsTemplate.Owner?.Username ?? "Global";
                throw new ValueNotFoundException($"Value '{request.ValueName}' not found in template '{existsTemplate.Name}' (Owner: {templateOwner})");
            }

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
}
