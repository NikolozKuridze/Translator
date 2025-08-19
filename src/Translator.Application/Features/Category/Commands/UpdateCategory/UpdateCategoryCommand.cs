using MediatR;

namespace Translator.Application.Features.Category.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string? Value,
    int? Order): IRequest;