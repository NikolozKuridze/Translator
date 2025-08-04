using MediatR;

namespace Translator.Application.Features.Translation.Commands.DeleteTranslation;

public record DeleteTranslationCommand(
    string Value, string LanguageCode) : IRequest;