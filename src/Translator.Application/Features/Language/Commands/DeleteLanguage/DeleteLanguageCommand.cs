using MediatR;

namespace Translator.Application.Features.Language.Commands.DeleteLanguage;

public record DeleteLanguageCommand(
        string Code
    ) : IRequest;