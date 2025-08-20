using MediatR;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Template;

public record GetCachedTemplatesCommand(PaginationRequest Pagination) : IRequest<PaginatedResponse<CachedTemplateInfo>>;