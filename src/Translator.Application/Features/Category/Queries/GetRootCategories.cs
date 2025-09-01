using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Queries;

public abstract class GetRootCategories
{
    public sealed record Query(PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<Response>>;

    public sealed record Response(
        Guid Id,
        string Value,
        string TypeName,
        string? Metadata,
        string? Shortcode,
        int? Order);

    public class Handler(IRepository<CategoryEntity> categoryRepository) : IRequestHandler<Query, PaginatedResponse<Response>>
    {
        public async Task<PaginatedResponse<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = categoryRepository
                .AsQueryable()
                .Where(c => c.ParentId == null)
                .Include(c => c.Type)
                .OrderBy(c => c.Order);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .Select(c => new Response(
                    c.Id,
                    c.Value,
                    c.Type.Name,
                    c.Metadata,
                    c.Shortcode,
                    c.Order
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Response>
            {
                Page = request.PaginationRequest.Page,
                PageSize = request.PaginationRequest.PageSize,
                TotalItems = totalCount,
                HasNextPage = (request.PaginationRequest.Page * request.PaginationRequest.PageSize) < totalCount,
                HasPreviousPage = request.PaginationRequest.Page > 1,
                Items = items
            };
        }
    }
}