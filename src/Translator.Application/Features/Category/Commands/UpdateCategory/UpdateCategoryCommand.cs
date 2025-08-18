using MediatR;

namespace Translator.Application.Features.Category.Commands.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string? Type,
    string? Value,
    int? Order,
    Guid? ParentId): IRequest;