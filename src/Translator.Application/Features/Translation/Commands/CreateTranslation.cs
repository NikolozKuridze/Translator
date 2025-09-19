using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Configurations.Constants;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;
using TranslationEntity = Translator.Domain.Entities.Translation;
using LanguageEntity = Translator.Domain.Entities.Language;

namespace Translator.Application.Features.Translation.Commands;

public abstract class CreateTranslation
{
    public record Command(
        string ValueName,
        string Translation,
        string LanguageCode
    ) : IRequest<Response>;

    public record Response(string Key, string Value);


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(command => command.ValueName)
                .NotEmpty()
                .WithMessage("Value cannot be empty")
                .MaximumLength(DatabaseConstants.Translation.VALUE_MAX_LENGTH)
                .WithMessage("Value cannot be longer than " + DatabaseConstants.Translation.VALUE_MAX_LENGTH);
        }
    }

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IRepository<LanguageEntity> _languageEntityRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly IValidator<Command> _validator;
        private readonly ValueCacheService _valueCacheService;
        private readonly IRepository<Value> _valueRepository;

        public Handler(
            IRepository<Value> valueRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<LanguageEntity> languageEntityRepository,
            ValueCacheService valueCacheService,
            IValidator<Command> validator)
        {
            _valueRepository = valueRepository;
            _translationRepository = translationRepository;
            _languageEntityRepository = languageEntityRepository;
            _valueCacheService = valueCacheService;
            _validator = validator;
        }

        public async Task<Response> Handle(Command request,
            CancellationToken cancellationToken)
        {
            await _validator.ValidateAndThrowAsync(request, cancellationToken);

            var valueNameHash = TemplateEntity.HashName(request.ValueName);

            var value = await _valueRepository
                .Where(t => t.Hash == valueNameHash)
                .Include(x => x.Translations)
                .ThenInclude(x => x.Language)
                .AsSingleQuery()
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (value is null)
                throw new ValueNotFoundException(request.ValueName);

            var languages = await _languageEntityRepository
                .Where(l => l.IsActive)
                .ToListAsync(cancellationToken);

            var detectedLanguages = LanguageDetector.DetectLanguages(request.Translation, languages);

            var requestedLanguage = detectedLanguages.FirstOrDefault(l => l.Code == request.LanguageCode);

            if (requestedLanguage == null)
            {
                var detectedCodes = detectedLanguages.Any()
                    ? string.Join(", ", detectedLanguages.Select(l => l.Code))
                    : "none";
                throw new LanguageMissMatchException(
                    request.Translation,
                    $"Requested language '{request.LanguageCode}' not compatible with text. Detected languages: [{detectedCodes}]"
                );
            }

            if (value.Translations.Any(x => x.TranslationValue == request.Translation ||
                                            x.Language.Code == request.LanguageCode))
                throw new TranslationAlreadyExistsException(request.ValueName);

            var translation = new TranslationEntity(value.Id, request.Translation)
            {
                Language = requestedLanguage
            };

            await _translationRepository.AddAsync(translation, cancellationToken);
            await _translationRepository.SaveChangesAsync(cancellationToken);

            if (await _valueCacheService.IsValueCached(value.Id))
            {
                await _valueCacheService.DeleteValueTranslationsAsync(value.Id);
                var updatedTranslations = await _translationRepository
                    .Where(t => t.Value.Id == value.Id)
                    .AsNoTracking()
                    .Include(t => t.Language)
                    .Select(t => new TranslationDto(
                        value.Key,
                        t.TranslationValue,
                        value.Id,
                        t.Language.Code
                    ))
                    .ToListAsync(cancellationToken);

                await _valueCacheService.SetTranslationsAsync(value.Id, value.Key, updatedTranslations);
            }

            return new Response(value.Key, translation.TranslationValue);
        }
    }
}