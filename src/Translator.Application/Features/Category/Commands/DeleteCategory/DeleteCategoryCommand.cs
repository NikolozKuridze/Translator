using MediatR;

namespace Translator.Application.Features.Category.Commands.DeleteCategory;

public record DeleteCategoryCommand(
    Guid Id) : IRequest;