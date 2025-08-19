using MediatR;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public sealed record CreateCategoryCommand(
    string Value,
    string TypeName,
    int? Order = null,
    Guid? ParentId = null
) : IRequest<Guid>;