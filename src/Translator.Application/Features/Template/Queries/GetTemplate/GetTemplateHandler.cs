using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.DataModels.Template;

namespace Translator.Application.Features.Template.Queries.GetTemplate;


public class GetFastTranslationsHandler : IRequestHandler<GetTemplateCommand, IEnumerable<TemplateTranslationDto>>
{
    private readonly IRepository<TemplateEntity> _templateRepository;

    public GetFastTranslationsHandler(IRepository<TemplateEntity> templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<IEnumerable<TemplateTranslationDto>> Handle(GetTemplateCommand request, CancellationToken cancellationToken)
    {
        var hash = TemplateEntity.HashName(request.TemplateName);

        var template = await (
            from t in _templateRepository
            where t.Hash == hash
            from tv in t.TemplateValues
            from tr in tv.Translations
                .Where(translation => translation.Language.Code == request.LanguageCode)
            select new TemplateTranslationDto(
                tv.Key,
                tr.Value ?? string.Empty)
        ).ToListAsync(cancellationToken);
        
        return template ?? throw new TemplateNotFoundException(request.TemplateName);
    }
}

public record TemplateTranslationDto(string Key, string Value);