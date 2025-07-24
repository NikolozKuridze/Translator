using MediatR;

namespace Translator.Application.Features.Language.DeleteLanguage;

public record DeleteLanguageCommand(
        string Code
    ) : IRequest;