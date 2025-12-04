using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.ValuesAdmin.Commands;

public abstract class AdminAddValueToTemplate
{
    
    public sealed record Command(
        string ValueName,
        Guid TemplateId) : IRequest;

    public class AddValueToTemplateHandler : IRequestHandler<Command>
    {
        private readonly TemplateCacheService _templateCacheService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<Value> _valueRepository;
        private readonly IRepository<User> _userRepository;

        public AddValueToTemplateHandler(
            IRepository<TemplateEntity> templateRepository,
            IRepository<Value> valueRepository,
            IRepository<User> userRepository,
            TemplateCacheService templateCacheService,
            ICurrentUserService currentUserService)
        {
            _templateRepository = templateRepository;
            _valueRepository = valueRepository;
            _userRepository = userRepository;
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
                .Where(t => t.Id == request.TemplateId)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is null)
                throw new TemplateNotFoundException(request.TemplateId);
            
            var valueAlreadyExists = existsTemplate
                .Values
                .Any(v => v.Hash == valueHash && v.OwnerId == existsTemplate.OwnerId);

            if (valueAlreadyExists)
                throw new InvalidOperationException($"Template already has value '{request.ValueName}'");

            var value = await _valueRepository
                .AsQueryable()
                .Include(v => v.Translations)
                .ThenInclude(t => t.Language)
                .Where(v => v.Hash == valueHash && v.OwnerId == existsTemplate.OwnerId)
                .SingleOrDefaultAsync(cancellationToken);

            if (value is null)
                throw new InvalidOperationException($"Value '{request.ValueName}' does not exist in database");

            existsTemplate.AddValue(value);

            var actualTranslations = existsTemplate.Values
                .SelectMany(v => v.Translations
                    .Select(t => new TranslationDto(
                        v.Key, t.TranslationValue, v.Id, t.Language.Code
                    )))
                .ToList();

            await _templateCacheService.DeleteTemplateAsync(request.TemplateId);
            await _templateCacheService.SetTemplateAsync(
                existsTemplate.Id,
                existsTemplate.Name,
                existsTemplate.OwnerId,
                existsTemplate.Owner?.Username,
                actualTranslations);

            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}