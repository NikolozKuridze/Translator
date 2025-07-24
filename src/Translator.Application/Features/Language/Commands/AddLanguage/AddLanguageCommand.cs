using MediatR;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

public record AddLanguageCommand(
        string Name,
        string Code,
        string UnicodeRange
    ) : IRequest<AddLanguageResponse>;
