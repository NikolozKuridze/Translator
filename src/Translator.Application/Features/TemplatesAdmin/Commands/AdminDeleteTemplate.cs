using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.TemplatesAdmin.Commands;

public abstract class AdminDeleteTemplate
{
    public sealed record Command(string TemplateName) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly TemplateCacheService _templateCacheService;
        private readonly IRepository<TemplateEntity> _templateRepository;

        public Handler(
            IRepository<TemplateEntity> templateRepository,
            TemplateCacheService templateCacheService)
        {
            _templateRepository = templateRepository;
            _templateCacheService = templateCacheService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var templateNameHash = TemplateEntity.HashName(request.TemplateName);

            var existsTemplate = await _templateRepository
                .Where(t => t.Hash == templateNameHash)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is null)
                throw new TemplateNotFoundException($"Template '{request.TemplateName}' not found");

            await _templateRepository.DeleteAsync([existsTemplate]);
            await _templateCacheService.DeleteTemplateAsync(existsTemplate.Id);
            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}