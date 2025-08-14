using MediatR;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public record CreateCategoryCommand(
    string Value,
    string Type,
    int? Order,
    Guid? ParentId
) : IRequest<Guid>;