using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Commands;

public abstract class DeleteTemplate
{
    public sealed record Command(string TemplateName) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly TemplateCacheService _templateCacheService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<User> _userRepository;

        public Handler(
            ICurrentUserService currentUserService,
            IRepository<TemplateEntity> templateRepository,
            IRepository<User> userRepository,
            TemplateCacheService templateCacheService)
        {
            _currentUserService = currentUserService;
            _templateRepository = templateRepository;
            _userRepository = userRepository;
            _templateCacheService = templateCacheService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var templateNameHash = TemplateEntity.HashName(request.TemplateName);

            var existsTemplate = await _templateRepository
                .Where(t => t.Hash == templateNameHash && t.OwnerId == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is null)
                throw new TemplateNotFoundException($"Template '{request.TemplateName}' not found or access denied");

            if (existsTemplate.OwnerId != userId.Value)
                throw new UnauthorizedOperationException("You can only delete your own templates");

            await _templateRepository.DeleteAsync([existsTemplate]);
            await _templateCacheService.DeleteTemplateAsync(existsTemplate.Id);
            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}