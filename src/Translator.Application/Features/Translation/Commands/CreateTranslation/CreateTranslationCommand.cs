using MediatR;
using Translator.Domain.Enums;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public record CreateTranslationCommand(
        string TemplateName,
        string TemplateValueName,
        string Value,
        Languages Language
    ) : IRequest<TranslationCreateResponse>;