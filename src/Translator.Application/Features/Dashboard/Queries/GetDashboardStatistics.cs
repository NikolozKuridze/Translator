using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Dashboard.Queries;

public abstract class GetDashboardStatistics
{
    public sealed record Query : IRequest<Response>;

    public sealed record Response(
        int TemplatesCount,
        int ValuesCount,
        int TranslationsCount,
        int LanguagesCount,
        int UsersCount,
        int GlobalTemplatesCount,
        int UserTemplatesCount,
        int GlobalValuesCount,
        int UserValuesCount
    );

    public class Handler : IRequestHandler<Query, Response>
    {
        private readonly IRepository<LanguageEntity> _languageRepository;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<TranslationEntity> _translationRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ValueEntity> _valueRepository;

        public Handler(
            IRepository<TemplateEntity> templateRepository,
            IRepository<LanguageEntity> languageRepository,
            IRepository<ValueEntity> valueRepository,
            IRepository<TranslationEntity> translationRepository,
            IRepository<User> userRepository)
        {
            _templateRepository = templateRepository;
            _languageRepository = languageRepository;
            _valueRepository = valueRepository;
            _translationRepository = translationRepository;
            _userRepository = userRepository;
        }

        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var templatesCount = await _templateRepository.AsQueryable()
                .CountAsync(cancellationToken);

            var valuesCount = await _valueRepository.AsQueryable()
                .CountAsync(cancellationToken);

            var translationsCount = await _translationRepository.AsQueryable()
                .CountAsync(cancellationToken);

            var languagesCount = await _languageRepository.AsQueryable()
                .CountAsync(l => l.IsActive, cancellationToken);

            var usersCount = await _userRepository.AsQueryable()
                .CountAsync(cancellationToken);

            var globalTemplatesCount = await _templateRepository.AsQueryable()
                .CountAsync(t => t.OwnerId == null, cancellationToken);

            var globalValuesCount = await _valueRepository.AsQueryable()
                .CountAsync(v => v.OwnerId == null, cancellationToken);

            return new Response(
                templatesCount,
                valuesCount,
                translationsCount,
                languagesCount,
                usersCount,
                globalTemplatesCount,
                templatesCount - globalTemplatesCount,
                globalValuesCount,
                valuesCount - globalValuesCount
            );
        }
    }
}