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

        var template = await _templateRepository
            .Where(t => t.Hash == hash)
            .Select(t => t.TemplateValues
                .Select(v => new TemplateTranslationDto(
                    v.Key,
                    v.Translations
                        .Where(tr => tr.Language == request.Language)
                        .Select(tr => tr.Value)
                        .FirstOrDefault() ?? string.Empty)))
            .SingleOrDefaultAsync(cancellationToken);

        return template ?? throw new TemplateNotFoundException(request.TemplateName);
    }
}

public record TemplateTranslationDto(string Key, string Value);