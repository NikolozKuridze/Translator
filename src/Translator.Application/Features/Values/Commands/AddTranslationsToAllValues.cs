using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using Translator.Infrastructure.External.DeepL;
using TranslationEntity = Translator.Domain.Entities.Translation;
using LanguageEntity = Translator.Domain.Entities.Language;

namespace Translator.Application.Features.Values.Commands;

public class AddTranslationsToAllValues
{
    public sealed record Command(
        string LanguageCode
    ) : IRequest<Response>;
    
    public sealed record Response(
        string LanguageCode,
        int CreatedTranslations,
        int ExistingTranslations);
    public class Handler : IRequestHandler<Command, Response>
    {
        private readonly IRepository<LanguageEntity> _languageEntityRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly ValueCacheService _valueCacheService;
        private readonly IRepository<Value> _valueRepository;
        private readonly IRepository<User> _userRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ITranslationService _translationService;
        public Handler(
            IRepository<Value> valueRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<LanguageEntity> languageEntityRepository,
            ValueCacheService valueCacheService,
            IRepository<User> userRepository,
            ICurrentUserService currentUserService,
            ITranslationService translationService)
        {
            _valueRepository = valueRepository;
            _translationRepository = translationRepository;
            _languageEntityRepository = languageEntityRepository;
            _valueCacheService = valueCacheService;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
            _translationService = translationService;
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

            if (request.LanguageCode == "en")
                throw new TranslationAlreadyExistsException("en");
            
            var languageIsActive = await _languageEntityRepository.AsQueryable()
                .FirstOrDefaultAsync(l => l.Code == request.LanguageCode && l.IsActive,
                    cancellationToken: cancellationToken);
            if (languageIsActive == null)
                throw new LanguageNotFoundException(request.LanguageCode);
                
            var values = await _valueRepository
                .Where(v => v.OwnerId == userId.Value)
                .Include(v => v.Translations)
                .ThenInclude(t => t.Language)
                .ToListAsync(cancellationToken: cancellationToken);

            if(values.Count == 0)
                throw new TranslationNotFoundException(request.LanguageCode);
            
            int existingTranslationsCount = 0;
            int createdTranslationsCount = 0;
            
            foreach (var value in values)
            {
                var translations = value.Translations;
                
                var englishTranslation = translations.FirstOrDefault(t => t.Language.Code == "en");
                if (englishTranslation == null)
                {
                    Console.WriteLine(value.Key);
                    throw new TranslationNotFoundException("en");
                }
                
                var newTranslation = translations.FirstOrDefault(t => t.Language.Code == request.LanguageCode);

                if (newTranslation == null)
                {
                    var generatedTranslation = await _translationService
                        .TranslateAsync(englishTranslation.TranslationValue, request.LanguageCode, cancellationToken);
                        
                    var createdTranslation = new TranslationEntity(value.Id, generatedTranslation)
                    {
                        Language = languageIsActive
                    };
                    await _translationRepository.AddAsync(createdTranslation, cancellationToken);
                    await _translationRepository.SaveChangesAsync(cancellationToken); 

                    createdTranslationsCount++;
                    Console.WriteLine($"Created Translations: {createdTranslationsCount} - value key: {value.Key}");
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
                                t.Language.Code ))
                            .ToListAsync(cancellationToken);
                        
                        await _valueCacheService.SetTranslationsAsync(
                            value.Id,
                            value.Key,
                            value.OwnerId,
                            value.Owner?.Username,
                            updatedTranslations);                    }
                }
                else
                {
                    existingTranslationsCount++;
                    Console.WriteLine($"Existing Translations: Count {existingTranslationsCount}");
                }
            }

            
            return new Response(request.LanguageCode,createdTranslationsCount,existingTranslationsCount);
        }
    }
}