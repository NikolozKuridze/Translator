using MediatR;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Redis.CacheServices;

namespace Translator.Application.Features.Caching.Queries.Value;

public record GetCachedValueCommand(PaginationRequest Pagination) : IRequest<PaginatedResponse<CachedValueInfo>>;