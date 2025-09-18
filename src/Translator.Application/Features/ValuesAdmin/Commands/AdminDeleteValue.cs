using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.ValuesAdmin.Commands;

public abstract class AdminDeleteValue
{
    public sealed record Command(
        Guid ValueId
    ) : IRequest;
    
    public class AdminDeleteValueByIdHandler : IRequestHandler<Command>
    {
        private readonly IRepository<Value> _valueRepository;
        private readonly ValueCacheService _valueCacheService;

        public AdminDeleteValueByIdHandler(
            IRepository<Value> valueRepository,
            ValueCacheService valueCacheService)
        {
            _valueRepository = valueRepository;
            _valueCacheService = valueCacheService;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var existsValue = await _valueRepository
                .Where(v => v.Id == request.ValueId)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsValue is null)
                throw new ValueNotFoundException($"Value with ID '{request.ValueId}' not found");

            await _valueRepository.DeleteAsync([existsValue]);
            await _valueCacheService.DeleteValueTranslationsAsync(existsValue.Id);
            await _valueRepository.SaveChangesAsync(cancellationToken);
        }
    }
}