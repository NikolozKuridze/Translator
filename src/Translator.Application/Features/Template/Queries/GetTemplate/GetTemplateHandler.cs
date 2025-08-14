using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Queries.GetTemplate;


public class GetTemplateHandler : IRequestHandler<GetTemplateCommand, IEnumerable<ValueDto>>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<TemplateEntity> _templateRepository;

    public GetTemplateHandler(IRepository<TemplateEntity> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<IEnumerable<ValueDto>> Handle(GetTemplateCommand request, CancellationToken cancellationToken)
    {
        if (request.AllTranslates)
        {
            var templateWithAllTranslates = await (
                from t in _templateRepository
                where t.Id == request.TemplateId
                from tv in t.Values
                from tr in tv.Translations
                select new ValueDto(
                    tv.Key,
                    tr.Value.Id,
                    tr.TranslationValue ?? string.Empty,
                    tr.Language.Code)
            ).ToArrayAsync(cancellationToken);

            if (!templateWithAllTranslates.Any())
                throw new TemplateNotFoundException(request.TemplateId.ToString());

            return templateWithAllTranslates;
        }

        var languageCode = string.IsNullOrEmpty(request.LanguageCode) 
            ? DefaultLanguageCode
            : request.LanguageCode;

        var template = await (
            from t in _templateRepository
            where t.Id == request.TemplateId
            from tv in t.Values
            from tr in tv.Translations
                .Where(translation => translation.Language.Code == languageCode)
            select new ValueDto(
                tv.Key,
                tr.Value.Id,
                tr.TranslationValue ?? string.Empty,
                tr.Language.Code)
        ).ToArrayAsync(cancellationToken);
        
        if (!template.Any())
            throw new TemplateNotFoundException(request.TemplateId.ToString());

        return template;
    }
}

public record TemplateDto(string Name, IList<ValueDto> Values);
public record ValueDto(string Key, Guid ValueId, string Value, string? LanguageCode);