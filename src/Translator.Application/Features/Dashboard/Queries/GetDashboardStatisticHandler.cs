using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;

using TemplateEntity = Translator.Domain.Entities.Template;
using LanguageEntity = Translator.Domain.Entities.Language;
using TranslationEntity = Translator.Domain.Entities.Translation;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Dashboard.Queries;

public class GetDashboardStatisticHandler : IRequestHandler<GetDashboardStatisticCommand, GetDashboardRequestResponse>
{
    private readonly IRepository<TemplateEntity> _templateRepository;
    private readonly IRepository<LanguageEntity> _languageRepository;
    private readonly IRepository<ValueEntity> _valueRepository;
    private readonly IRepository<TranslationEntity> _translationRepository;

    public GetDashboardStatisticHandler(
        IRepository<TemplateEntity> templateRepository,
        IRepository<LanguageEntity> languageRepository,
        IRepository<ValueEntity> valueRepository,
        IRepository<TranslationEntity> translationRepository)
    {
        _templateRepository = templateRepository;
        _languageRepository = languageRepository;
        _valueRepository = valueRepository;
        _translationRepository = translationRepository;
    }
    
    public async Task<GetDashboardRequestResponse> Handle(GetDashboardStatisticCommand request, CancellationToken cancellationToken)
    {

        var templatesCount = await _templateRepository.AsQueryable()
            .CountAsync(cancellationToken);

        var valuesCount = await _valueRepository.AsQueryable()
            .CountAsync(cancellationToken);

        var translationsCount = await _translationRepository.AsQueryable()
            .CountAsync(cancellationToken);

        var languagesCount = await _languageRepository.AsQueryable()
            .CountAsync(l => l.IsActive, cancellationToken);

        return new GetDashboardRequestResponse(
            templatesCount,
            valuesCount,
            translationsCount,
            languagesCount
        );
    }
}

public record GetDashboardRequestResponse(
    int TemplatesCount,
    int ValuesCount,
    int TranslationsCount,
    int LanguagesCount);