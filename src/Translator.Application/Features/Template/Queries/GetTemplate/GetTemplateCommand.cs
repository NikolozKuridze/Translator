using MediatR;

namespace Translator.Application.Features.Template.Queries.GetTemplate;

public record GetTemplateCommand(string TemplateName, string LanguageCode)
    : IRequest<IEnumerable<TemplateTranslationDto>>;
