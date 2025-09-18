using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Queries;

public abstract class SearchRootCategories
{
    public sealed record Query(string CategoryName,
        PaginationRequest PaginationRequest) :  IRequest<PaginatedResponse<GetRootCategories.Response>>;

    public class Handler(
        IRepository<CategoryEntity> _categoryRepository) : IRequestHandler<Query, PaginatedResponse<GetRootCategories.Response>>
    {
        public async Task<PaginatedResponse<GetRootCategories.Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = _categoryRepository
                .Where(c => string.IsNullOrEmpty(request.CategoryName) ||
                            c.Value.ToLower().Contains(request.CategoryName.ToLower()) ||
                            c.Value.ToLower() == request.CategoryName.ToLower());

            var totalItems = await query.CountAsync(cancellationToken);

            var categories = await query
                .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .Select(c => new GetRootCategories.Response(
                    c.Id,
                    c.Value,
                    c.Type.Name,
                    c.Metadata,
                    c.Shortcode,
                    c.Order))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<GetRootCategories.Response>()
            {
                Page = request.PaginationRequest.Page,
                PageSize = request.PaginationRequest.PageSize,
                TotalItems = totalItems,
                HasNextPage = request.PaginationRequest.Page * request.PaginationRequest.PageSize < totalItems,
                HasPreviousPage = request.PaginationRequest.Page > 1,
                Items = categories
            };
        }
    }
}