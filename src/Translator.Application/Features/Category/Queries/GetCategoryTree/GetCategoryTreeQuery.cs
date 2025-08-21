using MediatR;

namespace Translator.Application.Features.Category.Queries.GetCategoryTree;

public sealed record GetCategoryTreeQuery(Guid Id) : IRequest<CategoryTreeDto>;