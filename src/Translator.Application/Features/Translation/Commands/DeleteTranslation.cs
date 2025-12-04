using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;
using TranslationEntity = Translator.Domain.Entities.Translation;

namespace Translator.Application.Features.Translation.Commands;

public abstract class DeleteTranslation
{
    public record Command(
        string Value,
        string LanguageCode) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly ValueCacheService _valueCacheService;
        private readonly IRepository<Value> _valueRepository;

        public Handler(
            IRepository<Value> valueRepository,
            ValueCacheService valueCacheService,
            IRepository<TranslationEntity> translationRepository)
        {
            _valueRepository = valueRepository;
            _valueCacheService = valueCacheService;
            _translationRepository = translationRepository;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var valueHash = TemplateEntity.HashName(request.Value);
            var value = await _valueRepository
                .AsQueryable()
                .Where(x => x.Hash == valueHash)
                .Include(x => x.Translations)
                .ThenInclude(x => x.Language)
                .SingleOrDefaultAsync(cancellationToken);

            if (value is null)
                throw new ValueNotFoundException(request.Value);

            var translations = value.Translations.ToList();

            var removedTranslation = translations
                .FirstOrDefault(t => t.Language.Code == request.LanguageCode);

            if (removedTranslation is null)
                throw new TranslationNotFoundException(request.LanguageCode);

            if (await _valueCacheService.IsValueCached(value.Id))
            {
                await _valueCacheService.DeleteValueTranslationsAsync(value.Id);

                var updatedTranslations = translations
                    .Where(t => t.Language.Code != request.LanguageCode)
                    .ToList();

                var translationsDto = updatedTranslations
                    .Select(t => new TranslationDto(
                        request.Value, t.TranslationValue, value.Id, t.Language.Code
                    ))
                    .ToList();

                await _valueCacheService.SetTranslationsAsync(
                    value.Id,
                    value.Key,
                    value.OwnerId,
                    value.Owner?.Username,
                    translationsDto);            }

            await _translationRepository.DeleteAsync([removedTranslation]);
            await _translationRepository.SaveChangesAsync(cancellationToken);
        }
    }
}