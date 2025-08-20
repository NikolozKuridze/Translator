using MediatR;

namespace Translator.Application.Features.Category.Queries.GetRootCategories;

public sealed record GetRootCategoriesQuery : IRequest<IEnumerable<RootCategoryDto>>;