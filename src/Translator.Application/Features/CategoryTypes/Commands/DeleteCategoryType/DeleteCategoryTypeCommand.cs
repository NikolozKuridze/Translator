using MediatR;

namespace Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;

public record DeleteCategoryTypeCommand(
    string Type) : IRequest;