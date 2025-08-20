using MediatR;

namespace Translator.Application.Features.Category.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(
    Guid Id) : IRequest;