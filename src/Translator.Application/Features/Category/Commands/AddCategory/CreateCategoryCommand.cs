using MediatR;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public record CreateCategoryCommand(
    string Value,
    int Order,
    string Type,
    Guid? ParentId
) : IRequest<Guid>;