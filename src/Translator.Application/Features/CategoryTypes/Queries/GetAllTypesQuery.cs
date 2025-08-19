using MediatR;

namespace Translator.Application.Features.CategoryTypes.Queries;

public record GetAllTypesQuery : IRequest<List<string>>;