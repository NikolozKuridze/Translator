using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries;

public abstract class GetTemplatesByIds
{
    public sealed record Command(
        List<Guid> TemplateIds,
        string? LanguageCode
    ) : IRequest<IEnumerable<Response>>;

    public sealed record Response(
        string TemplateId,
        string TemplateName,
        List<TemplateValuesDto> Values);

    public sealed record TemplateValuesDto(
        string Key,
        string Value);

    public class Handler : IRequestHandler<Command, IEnumerable<Response>>
    {
        private const string DefaultLanguageCode = "en";
        private readonly ICurrentUserService _currentUserService;
        private readonly TemplateCacheService _templateCacheService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<User> _userRepository;

        public Handler(
            ICurrentUserService currentUserService,
            IRepository<TemplateEntity> templateRepository,
            IRepository<User> userRepository,
            TemplateCacheService templateCacheService)
        {
            _currentUserService = currentUserService;
            _templateRepository = templateRepository;
            _userRepository = userRepository;
            _templateCacheService = templateCacheService;
        }

        public async Task<IEnumerable<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            if (request.TemplateIds.Count == 0)
                return [];

            // Get accessible templates with their names
            var accessibleTemplates = await _templateRepository
                .Where(t => request.TemplateIds.Contains(t.Id) &&
                            (t.OwnerId == userId.Value || t.OwnerId == null))
                .Select(t => new { t.Id, t.Name })
                .ToListAsync(cancellationToken);

            if (accessibleTemplates.Count == 0)
                return [];

            var languageCode = string.IsNullOrEmpty(request.LanguageCode)
                ? DefaultLanguageCode
                : request.LanguageCode;

            var allResults = new List<Response>();
            var uncachedTemplateIds = new List<Guid>();

            // Handle cached templates
            foreach (var template in accessibleTemplates)
            {
                var cachedResult = await _templateCacheService.GetTranslationsAsync(template.Id);
                if (cachedResult != null)
                {
                    var cachedValues = cachedResult.Translations
                        .Where(t => t.LanguageCode == languageCode)
                        .Select(t => new TemplateValuesDto(t.Key, t.Value))
                        .ToList();

                    allResults.Add(new Response(
                        template.Id.ToString(),
                        template.Name,
                        cachedValues
                    ));
                }
                else
                {
                    uncachedTemplateIds.Add(template.Id);
                }
            }

            // Handle uncached templates from database
            if (uncachedTemplateIds.Count > 0)
            {
                var databaseResults = await HandleDatabaseQuery(uncachedTemplateIds, languageCode, cancellationToken);
                allResults.AddRange(databaseResults);
            }

            return allResults;
        }

        private async Task<IEnumerable<Response>> HandleDatabaseQuery(
            List<Guid> templateIds,
            string languageCode,
            CancellationToken cancellationToken)
        {
            // Group query to get template info and values together
            var query = from t in _templateRepository
                where templateIds.Contains(t.Id)
                select new
                {
                    TemplateId = t.Id,
                    TemplateName = t.Name,
                    Values = t.Values
                        .SelectMany(tv => tv.Translations
                            .Where(tr => tr.Language.Code == languageCode)
                            .Select(tr => new TemplateValuesDto(
                                tv.Key,
                                tr.TranslationValue ?? string.Empty
                            )))
                        .ToList()
                };

            var results = await query.ToListAsync(cancellationToken);

            return results.Select(r => new Response(
                r.TemplateId.ToString(),
                r.TemplateName,
                r.Values
            ));
        }
    }
}