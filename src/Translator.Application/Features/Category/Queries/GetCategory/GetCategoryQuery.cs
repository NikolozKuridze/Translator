using MediatR;

namespace Translator.Application.Features.Category.Queries.GetCategory;

public sealed record GetCategoryQuery(Guid Id) : IRequest<CategoryReadDto>;