using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Queries.GetTemplate;


public class GetTemplateHandler : IRequestHandler<GetTemplateCommand, IEnumerable<TemplateTranslationDto>>
{
    private const string DefaultLanguageCode = "en";
    private readonly IRepository<TemplateEntity> _templateRepository;

    public GetTemplateHandler(IRepository<TemplateEntity> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<IEnumerable<TemplateTranslationDto>> Handle(GetTemplateCommand request, CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(request.LanguageCode) 
            ? DefaultLanguageCode 
            : request.LanguageCode;
        
        var hash = TemplateEntity.HashName(request.TemplateName);

        var template = await (
            from t in _templateRepository
            where t.Hash == hash
            from tv in t.Values
            from tr in tv.Translations
                .Where(translation => translation.Language.Code == languageCode)
            select new TemplateTranslationDto(
                tv.Key,
                tr.TranslationValue ?? string.Empty)
            ).ToArrayAsync(cancellationToken);
        
        return template ?? throw new TemplateNotFoundException(request.TemplateName);
    }
}

public record TemplateTranslationDto(string Key, string Value);