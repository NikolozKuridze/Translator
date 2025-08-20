using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using Translator.Infrastructure.Database.Redis.CacheServices;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries.GetTemplate;

public class GetTemplateHandler : IRequestHandler<GetTemplateCommand, PaginatedResponse<ValueDto>>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly TemplateCacheService _templateCacheService;

    public GetTemplateHandler(
        IRepository<TemplateEntity> templateRepository,
        TemplateCacheService templateCacheService)
    {
        _templateRepository = templateRepository;
        _templateCacheService = templateCacheService;
    }

    public async Task<PaginatedResponse<ValueDto>> Handle(GetTemplateCommand request, CancellationToken cancellationToken)
    {
        
        var cachedResult = await _templateCacheService.GetTranslationsAsync(request.TemplateId);
        if (cachedResult != null)
            return await HandleCachedResult(cachedResult, request);
        
        return await HandleDatabaseQuery(request, cancellationToken);
    }

    private Task<PaginatedResponse<ValueDto>> HandleCachedResult(
        TemplateCacheDto cachedResult, 
        GetTemplateCommand request)
    {
        IEnumerable<ValueDto> translations;

        if (request.AllTranslates)
        {
            translations = cachedResult.Translations
                .Select(t => new ValueDto(t.Key, t.ValueId, t.Value, t.LanguageCode));
        }
        else
        {
            translations = cachedResult.Translations
                .Where(t => t.LanguageCode == request.LanguageCode)
                .Select(t => new ValueDto(t.Key, t.ValueId, t.Value, t.LanguageCode));
        }

        if (!string.IsNullOrEmpty(request.Pagination?.Search))
        {
            translations = translations.Where(t =>
                t.Key.Contains(request.Pagination.Search, StringComparison.OrdinalIgnoreCase) ||
                t.Value.Contains(request.Pagination.Search, StringComparison.OrdinalIgnoreCase));
        }

        
        translations = ApplySorting(translations, request.Pagination!)
            .ToList();
        
        var totalItems = translations.Count();
        var pagedItems = translations
            .Skip((request.Pagination!.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToList();

        return Task.FromResult(new PaginatedResponse<ValueDto>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = totalItems,
            HasNextPage = request.Pagination.Page * request.Pagination.PageSize < totalItems,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = pagedItems
        });
    }

    private async Task<PaginatedResponse<ValueDto>> HandleDatabaseQuery(
        GetTemplateCommand request, 
        CancellationToken cancellationToken)
    {
         var languageCode = string.IsNullOrEmpty(request.LanguageCode) 
        ? DefaultLanguageCode 
        : request.LanguageCode;

        IQueryable<ValueDto> query;

        if (request.AllTranslates)
        {
            query = from t in _templateRepository
                    where t.Id == request.TemplateId
                    from tv in t.Values
                    from tr in tv.Translations
                    select new ValueDto(
                        tv.Key,
                        tr.Value.Id,
                        tr.TranslationValue ?? string.Empty,
                        tr.Language.Code);
        }
        else
        {
            query = from t in _templateRepository
                    where t.Id == request.TemplateId
                    from tv in t.Values
                    from tr in tv.Translations
                        .Where(translation => translation.Language.Code == languageCode)
                    select new ValueDto(
                        tv.Key,
                        tr.Value.Id,
                        tr.TranslationValue ?? string.Empty,
                        tr.Language.Code);
        }

        if (!string.IsNullOrEmpty(request.Pagination?.Search))
        {
            query = query.Where(v => 
                v.Key.Contains(request.Pagination.Search) ||
                v.Value.Contains(request.Pagination.Search));
        }

        var allItems = await query.ToListAsync(cancellationToken);

        if (allItems.Count == 0)
            throw new TemplateNotFoundException(request.TemplateId.ToString());

        var sortedItems = ApplySorting(allItems, request.Pagination!).ToArray();

        var totalItems = sortedItems.Length;
        var pagedItems = sortedItems
            .Skip((request.Pagination!.Page - 1) * request.Pagination.PageSize)
            .Take(request.Pagination.PageSize)
            .ToList();

        return new PaginatedResponse<ValueDto>
        {
            Page = request.Pagination.Page,
            PageSize = request.Pagination.PageSize,
            TotalItems = totalItems,
            HasNextPage = request.Pagination.Page * request.Pagination.PageSize < totalItems,
            HasPreviousPage = request.Pagination.Page > 1,
            Items = pagedItems
        };
    }

    private static IEnumerable<ValueDto> ApplySorting(
        IEnumerable<ValueDto> query, 
        PaginationRequest pagination)
    {
        if (string.IsNullOrEmpty(pagination.SortBy))
            return query.OrderBy(x => x.Key); 

        var isDescending = pagination.SortDirection?.ToLower() == "desc";

        return pagination.SortBy.ToLower() switch
        {
            "key" => isDescending ? query.OrderByDescending(x => x.Key) : query.OrderBy(x => x.Key),
            "value" => isDescending ? query.OrderByDescending(x => x.Value) : query.OrderBy(x => x.Value),
            "languagecode" => isDescending ? query.OrderByDescending(x => x.LanguageCode) : query.OrderBy(x => x.LanguageCode),
            _ => query.OrderBy(x => x.Key)
        };
    }
    
}

public record TemplateDto(string Name, IList<ValueDto> Values);
public record ValueDto(string Key, Guid ValueId, string Value, string? LanguageCode);
