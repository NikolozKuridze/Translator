using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Values.Commands;

public abstract class DeleteValue
{
    public sealed record Command(
        string ValueName
    ) : IRequest;
    
    public class DeleteValueHandler : IRequestHandler<Command>
    {
        private readonly IRepository<Value> _valueRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ValueCacheService _valueCacheService;
        private readonly ICurrentUserService _currentUserService;

        public DeleteValueHandler(
            IRepository<Value> valueRepository,
            IRepository<User> userRepository,
            ValueCacheService valueCacheService,
            ICurrentUserService currentUserService)
        {
            _valueRepository = valueRepository;
            _userRepository = userRepository;
            _valueCacheService = valueCacheService;
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

            var valueNameHash = TemplateEntity.HashName(request.ValueName);

            var existsValue = await _valueRepository
                .Where(v => v.Hash == valueNameHash && v.OwnerId == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsValue is null)
                throw new ValueNotFoundException($"Value '{request.ValueName}' not found for the current user");

            if (existsValue.OwnerId != userId.Value)
                throw new UnauthorizedOperationException("You can only delete your own values");

            await _valueRepository.DeleteAsync([existsValue]);
            await _valueCacheService.DeleteValueTranslationsAsync(existsValue.Id);
            await _valueRepository.SaveChangesAsync(cancellationToken);
        }
    }

}