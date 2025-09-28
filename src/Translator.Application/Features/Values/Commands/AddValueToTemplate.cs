using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Values.Commands;

public abstract class AddValueToTemplate
{
    public sealed record Command(
        string ValueName,
        Guid TemplateId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Value> _valueRepository;

        public Handler(
            IRepository<Value> valueRepository,
            IRepository<User> userRepository,
            IRepository<TemplateEntity> templateRepository,
            ICurrentUserService currentUserService)
        {
            _valueRepository = valueRepository;
            _userRepository = userRepository;
            _templateRepository = templateRepository;
            _currentUserService = currentUserService;
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

            var valueHash = TemplateEntity.HashName(request.ValueName);

            var existsTemplate = await _templateRepository
                .AsQueryable()
                .Include(t => t.Values)
                .ThenInclude(v => v.Translations)
                .ThenInclude(tr => tr.Language)
                .Where(t => t.Id == request.TemplateId &&
                            (t.OwnerId == userId.Value || t.OwnerId == null))
                .SingleOrDefaultAsync(cancellationToken);

            if (existsTemplate is null)
                throw new TemplateNotFoundException(request.TemplateId);

            if (existsTemplate.OwnerId != userId.Value)
                throw new UnauthorizedOperationException("You can only modify your own templates");

            var existsValue = existsTemplate
                .Values
                .SingleOrDefault(x => x.Hash == valueHash);

            if (existsValue is null)
                throw new ValueNotFoundException(request.ValueName);

            if (existsValue.OwnerId != userId.Value && existsValue.OwnerId != null)
                throw new UnauthorizedOperationException("You can only add your own values or global values");

            existsTemplate.Values.Add(existsValue);
            await _templateRepository.UpdateAsync(existsTemplate);
            await _templateRepository.SaveChangesAsync(cancellationToken);
        }
    }
}