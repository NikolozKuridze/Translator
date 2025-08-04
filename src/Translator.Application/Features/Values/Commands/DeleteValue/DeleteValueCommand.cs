using MediatR;

namespace Translator.Application.Features.Values.Commands.DeleteValue;

public record DeleteValueCommand(
        string ValueName
    ) : IRequest;