using MediatR;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public record CreateCategoryCommand(
    string Value,
    string Type,
    int? Order = null,
    Guid? ParentId = null
) : IRequest<Guid>;