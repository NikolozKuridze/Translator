using MediatR;

namespace Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;

public record DeleteCategoryTypeCommand(
    string TypeName) : IRequest;