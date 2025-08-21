using MediatR;

namespace Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;

public sealed record DeleteCategoryTypeCommand(
    IEnumerable<string> TypeNames) : IRequest;