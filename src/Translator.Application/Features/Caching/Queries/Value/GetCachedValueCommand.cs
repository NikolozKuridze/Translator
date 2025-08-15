using MediatR;
using Translator.Application.Features.Caching.Queries.Template;

namespace Translator.Application.Features.Caching.Queries.Value;

public record GetCachedValueCommand(int Skip, int Take) : IRequest<IEnumerable<GetCachedValueResponse>>;