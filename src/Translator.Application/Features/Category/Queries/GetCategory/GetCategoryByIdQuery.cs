using MediatR;
using Translator.Application.Features.Category.Queries.GetCategory;

namespace Translator.Application.Features.Category.Queries;

public record GetCategoryByIdQuery(Guid Id) : IRequest<GetCategoryResponse>;