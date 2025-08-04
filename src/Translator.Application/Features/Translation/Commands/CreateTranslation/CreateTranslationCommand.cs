using MediatR;

namespace Translator.Application.Features.Translation.Commands.CreateTranslation;

public record CreateTranslationCommand(
    string ValueName,
    string Translation,
    string LanguageCode
    ) : IRequest<TranslationCreateResponse>;