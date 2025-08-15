using MediatR;

namespace Translator.Application.Features.Caching.Queries.Template;

public record GetCachedTemplatesCommand(int Skip, int Take) : IRequest<IEnumerable<GetCachedTemplatesResponse>>;