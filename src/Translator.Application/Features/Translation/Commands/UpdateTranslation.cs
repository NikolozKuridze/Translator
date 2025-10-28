using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TranslationEntity = Translator.Domain.Entities.Translation;
using LanguageEntity = Translator.Domain.Entities.Language;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Translation.Commands;

public abstract class UpdateTranslation
{
    public sealed record Command(
        string ValueKey,
        string LanguageCode,
        string TranslationValue
        ) : IRequest<Response>;

    public sealed record Response(
        string Key,
        string Value
        );

    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly ValueCacheService _valueCacheService;
        private readonly IRepository<Value> _valueRepository;
        private readonly IRepository<LanguageEntity> _languageEntityRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;

        public Handler(
            IRepository<TranslationEntity> translationRepository,
            ValueCacheService valueCacheService,
            IRepository<Value> valueRepository,
            IRepository<LanguageEntity> languageEntityRepository,
            IRepository<User> userRepository, ICurrentUserService currentUserService)
        {
            _translationRepository = translationRepository;
            _valueCacheService = valueCacheService;
            _valueRepository = valueRepository;
            _languageEntityRepository = languageEntityRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);
    
            var valueHash = TemplateEntity.HashName(request.ValueKey);
            var value = await _valueRepository
                .AsQueryable()
                .Where(x => x.Hash == valueHash)
                .Include(x => x.Translations)
                .ThenInclude(x => x.Language)
                .SingleOrDefaultAsync(cancellationToken);
    
            if (value is null)
                throw new ValueNotFoundException(request.ValueKey);
    
            var updatedTranslation = value.Translations
                .FirstOrDefault(t => t.Language.Code == request.LanguageCode);
    
            if (updatedTranslation is null)
                throw new TranslationNotFoundException(request.LanguageCode);
    
            var language = await _languageEntityRepository
                .Where(l => l.Code == request.LanguageCode &&  l.IsActive)
                .SingleOrDefaultAsync(cancellationToken);

            if (language is null)
                throw new LanguageNotFoundException(request.LanguageCode);
            
            var detectedLanguages = LanguageDetector.DetectLanguages(request.TranslationValue, [language]);

            if (detectedLanguages.Count == 0)
                throw new UnkownLanguageException($"No compatible language found for value: {request.TranslationValue}");

            updatedTranslation.TranslationValue = request.TranslationValue;
    
            if (await _valueCacheService.IsValueCached(value.Id))
            {
                await _valueCacheService.DeleteValueTranslationsAsync(value.Id);

                var translationsDto = value.Translations 
                    .Select(t => new TranslationDto(
                        request.ValueKey, t.TranslationValue, value.Id, t.Language.Code
                    ))
                    .ToList();
            
                await _valueCacheService.SetTranslationsAsync(value.Id, value.Key, translationsDto);
            }
    
            await _translationRepository.UpdateAsync(updatedTranslation);
            await _translationRepository.SaveChangesAsync(cancellationToken);
    
            return new Response(value.Key, updatedTranslation.TranslationValue);
        }
    }
}