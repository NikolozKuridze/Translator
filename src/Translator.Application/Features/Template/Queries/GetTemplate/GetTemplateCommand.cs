using MediatR;
using Translator.Domain.Enums;

namespace Translator.Application.Features.Template.Queries.GetTemplate;

public record GetTemplateCommand(string TemplateName, Languages Language)
    : IRequest<IEnumerable<TemplateTranslationDto>>;
