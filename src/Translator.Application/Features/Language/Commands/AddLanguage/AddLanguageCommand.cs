using MediatR;

namespace Translator.Application.Features.Language.Commands.AddLanguage;

public record AddLanguageCommand(
        string Code
    ) : IRequest<AddLanguageResponse>;
