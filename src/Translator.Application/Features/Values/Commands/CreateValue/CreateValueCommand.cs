using MediatR;

namespace Translator.Application.Features.Values.Commands.CreateValue;

public record CreateValueCommand(
        string Key, 
        string Value
    ) : IRequest;