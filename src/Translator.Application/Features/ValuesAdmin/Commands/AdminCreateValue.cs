using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.ValuesAdmin.Commands;

public abstract class AdminCreateValue
{
    public record Command(
        string Key,
        string Value,
        string? Username = null
    ) : IRequest;

    public class AdminCreateValueHandler : IRequestHandler<Command>
    {
        private readonly IRepository<LanguageEntity> _languageEntityRepository;
        private readonly IRepository<Value> _templateValueRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly IRepository<User> _userRepository;

        public AdminCreateValueHandler(
            IRepository<Value> templateValueRepository,
            IRepository<LanguageEntity> languageEntityRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<User> userRepository)
        {
            _templateValueRepository = templateValueRepository;
            _languageEntityRepository = languageEntityRepository;
            _translationRepository = translationRepository;
            _userRepository = userRepository;
        }

        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            Guid? ownerId = null;

            if (!string.IsNullOrEmpty(request.Username))
            {
                var user = await _userRepository
                    .Where(u => u.Username == request.Username)
                    .SingleOrDefaultAsync(cancellationToken);

                if (user == null)
                    throw new UserNotFoundException($"User with username '{request.Username}' not found");

                ownerId = user.Id;
            }

            var existsTemplateValueHash = TemplateEntity.HashName(request.Key);

            var existsValue = await _templateValueRepository
                .Where(t => t.Hash == existsTemplateValueHash && t.OwnerId == ownerId)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsValue is not null)
            {
                var scope = ownerId.HasValue ? $" for user '{request.Username}'" : " globally";
                throw new ValueAlreadyExistsException($"Value '{request.Key}' already exists{scope}");
            }

            var languages = await _languageEntityRepository
                .Where(l => l.IsActive)
                .ToListAsync(cancellationToken);

            var detectedLanguages = LanguageDetector.DetectLanguages(request.Value, languages);

            if (!detectedLanguages.Any())
                throw new UnkownLanguageException($"No compatible language found for value: {request.Value}");

            var selectedLanguage =
                detectedLanguages.FirstOrDefault(l => l.Code.Equals("en", StringComparison.OrdinalIgnoreCase));

            var value = new Value(request.Key, ownerId);

            var translation = new TranslationEntity(value.Id, request.Value)
            {
                Language = selectedLanguage ?? detectedLanguages.First()
            };

            await _templateValueRepository.AddAsync(value, cancellationToken);
            await _translationRepository.AddAsync(translation, cancellationToken);

            await _templateValueRepository.SaveChangesAsync(cancellationToken);
        }
    }
}
