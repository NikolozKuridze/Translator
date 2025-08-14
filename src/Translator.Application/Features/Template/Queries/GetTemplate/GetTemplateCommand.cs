using MediatR;

namespace Translator.Application.Features.Template.Queries.GetTemplate;

public record GetTemplateCommand(Guid TemplateId, string? LanguageCode, bool AllTranslates)
    : IRequest<IEnumerable<TemplateDto>>;
