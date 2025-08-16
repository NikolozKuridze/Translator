using MediatR;

namespace Translator.Application.Features.Category.Queries.GetCategory;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryReadDto>;