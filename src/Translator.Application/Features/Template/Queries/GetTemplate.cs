using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries;

public abstract class GetTemplate
{
    public sealed record Command(
        Guid TemplateId,
        string? LanguageCode,
        bool AllTranslates,
        PaginationRequest? Pagination)
        : IRequest<PaginatedResponse<Response>>;

    public sealed record Response(
        string Key,
        Guid ValueId,
        string Value,
        string? LanguageCode);

    public class Handler : IRequestHandler<Command, PaginatedResponse<Response>>
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

        public async Task<PaginatedResponse<Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var templateExists = await _templateRepository
                .Where(t => t.Id == request.TemplateId && 
                           (t.OwnerId == userId.Value || t.OwnerId == null))
                .AnyAsync(cancellationToken);

            if (!templateExists)
                throw new TemplateNotFoundException($"Template with ID '{request.TemplateId}' not found or access denied");

            var pagination = request.Pagination ?? new PaginationRequest(1, 10, null, null, null, null, null);

            var cachedResult = await _templateCacheService.GetTranslationsAsync(request.TemplateId);
            if (cachedResult != null)
                return await HandleCachedResult(cachedResult, request, pagination);

            return await HandleDatabaseQuery(request, pagination, cancellationToken);
        }

        private Task<PaginatedResponse<Response>> HandleCachedResult(
            TemplateCacheDto cachedResult,
            Command request,
            PaginationRequest pagination)
        {
            IEnumerable<Response> translations;

            if (request.AllTranslates)
                translations = cachedResult.Translations
                    .Select(t => new Response(t.Key, t.ValueId, t.Value, t.LanguageCode));
            else
                translations = cachedResult.Translations
                    .Where(t => t.LanguageCode == request.LanguageCode)
                    .Select(t => new Response(t.Key, t.ValueId, t.Value, t.LanguageCode));

            if (!string.IsNullOrEmpty(pagination.Search))
                translations = translations.Where(t =>
                    t.Key.Contains(pagination.Search, StringComparison.OrdinalIgnoreCase) ||
                    t.Value.Contains(pagination.Search, StringComparison.OrdinalIgnoreCase));

            translations = ApplySorting(translations, pagination)
                .ToList();

            var totalItems = translations.Count();
            var pagedItems = translations
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            return Task.FromResult(new PaginatedResponse<Response>
            {
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalItems,
                HasNextPage = pagination.Page * pagination.PageSize < totalItems,
                HasPreviousPage = pagination.Page > 1,
                Items = pagedItems
            });
        }

        private async Task<PaginatedResponse<Response>> HandleDatabaseQuery(
            Command request,
            PaginationRequest pagination,
            CancellationToken cancellationToken)
        {
            var languageCode = string.IsNullOrEmpty(request.LanguageCode)
                ? DefaultLanguageCode
                : request.LanguageCode;

            IQueryable<Response> query;

            if (request.AllTranslates)
                query = from t in _templateRepository
                    where t.Id == request.TemplateId
                    from tv in t.Values
                    from tr in tv.Translations
                    select new Response(
                        tv.Key,
                        tr.Value.Id,
                        tr.TranslationValue ?? string.Empty,
                        tr.Language.Code);
            else
                query = from t in _templateRepository
                    where t.Id == request.TemplateId
                    from tv in t.Values
                    from tr in tv.Translations
                        .Where(translation => translation.Language.Code == languageCode)
                    select new Response(
                        tv.Key,
                        tr.Value.Id,
                        tr.TranslationValue ?? string.Empty,
                        tr.Language.Code);

            if (!string.IsNullOrEmpty(pagination.Search))
                query = query.Where(v =>
                    v.Key.Contains(pagination.Search) ||
                    v.Value.Contains(pagination.Search));

            var allItems = await query.ToListAsync(cancellationToken);

            if (allItems.Count == 0)
                throw new TemplateNotFoundException($"Template with ID '{request.TemplateId}' has no translations or does not exist");

            var sortedItems = ApplySorting(allItems, pagination).ToList();

            var totalItems = sortedItems.Count;
            var pagedItems = sortedItems
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            return new PaginatedResponse<Response>
            {
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalItems = totalItems,
                HasNextPage = pagination.Page * pagination.PageSize < totalItems,
                HasPreviousPage = pagination.Page > 1,
                Items = pagedItems
            };
        }

        private static IEnumerable<Response> ApplySorting(
            IEnumerable<Response> query,
            PaginationRequest pagination)
        {
            if (string.IsNullOrEmpty(pagination.SortBy))
                return query.OrderBy(x => x.Key);

            var isDescending = pagination.SortDirection?.ToLower() == "desc";

            return pagination.SortBy.ToLower() switch
            {
                "key" => isDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key),
                "value" => isDescending ? query.OrderByDescending(x => x.Value) : query.OrderBy(x => x.Value),
                "languagecode" => isDescending
                    ? query.OrderByDescending(x => x.LanguageCode)
                    : query.OrderBy(x => x.LanguageCode),
                _ => query.OrderBy(x => x.Key)
            };
        }
    }
}
