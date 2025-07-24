using MediatR;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public record CreateTranslationCommand(
        string TemplateName,
        string TemplateValueName,
        string Value,
        string LanguageCode
    ) : IRequest<TranslationCreateResponse>;