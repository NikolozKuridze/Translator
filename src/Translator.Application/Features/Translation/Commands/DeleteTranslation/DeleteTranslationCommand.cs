using MediatR;

namespace Translator.Application.Features.Translation.Commands.DeleteTranslation;

public record DeleteTranslationCommand(
    string Template, 
    string TemplateValue,
    string Value) : IRequest;