using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Values.Commands;

public abstract class CreateValue
{
    public sealed record Command(
        string Key,
        string Value
    ) : IRequest<Response>;

    public sealed record Response(
        Guid Id);

    public class CreateValueHandler : IRequestHandler<Command, Response>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<LanguageEntity> _languageEntityRepository;
        private readonly IRepository<Value> _templateValueRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly IRepository<User> _userRepository;

        public CreateValueHandler(
            IRepository<Value> templateValueRepository,
            IRepository<LanguageEntity> languageEntityRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _templateValueRepository = templateValueRepository;
            _languageEntityRepository = languageEntityRepository;
            _translationRepository = translationRepository;
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

            var existsTemplateValueHash = TemplateEntity.HashName(request.Key);

            var existsValue = await _templateValueRepository
                .Where(v => v.Hash == existsTemplateValueHash && v.OwnerId == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (existsValue is not null)
                throw new ValueAlreadyExistsException($"Value '{request.Key}' already exists for this user");

            var languages = await _languageEntityRepository
                .Where(l => l.IsActive)
                .ToListAsync(cancellationToken);

            var detectedLanguages = LanguageDetector.DetectLanguages(request.Value, languages);

            if (detectedLanguages.Count == 0)
                throw new UnkownLanguageException($"No compatible language found for value: {request.Value}");

            var selectedLanguage =
                detectedLanguages.FirstOrDefault(l => l.Code.Equals("en", StringComparison.OrdinalIgnoreCase));

            var value = new Value(request.Key, userId.Value);

            var translation = new TranslationEntity(value.Id, request.Value)
            {
                Language = selectedLanguage ?? detectedLanguages.First()
            };

            await _templateValueRepository.AddAsync(value, cancellationToken);
            await _translationRepository.AddAsync(translation, cancellationToken);

            await _templateValueRepository.SaveChangesAsync(cancellationToken);
            
            return new Response(value.Id);
        }
    }
}