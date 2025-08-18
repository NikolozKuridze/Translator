using MediatR;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategoryTree;

public record GetCategoryTreeCommand(Guid Id) : IRequest<IEnumerable<CategoryEntity>>;