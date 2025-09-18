using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.ValuesAdmin.Queries;

public abstract class AdminGetAllValues
{
    public sealed record Command(
        PaginationRequest Pagination) : 
        IRequest<PaginatedResponse<Response>>;

    public sealed record Response(
        string Key,
        Guid ValueId,
        int TranslationsCount,
        DateTimeOffset CreatedAt,
        Guid? OwnerId,
        string OwnerName,
        string OwnershipType
    );

    public class AdminGetAllValuesHandler : IRequestHandler<Command, PaginatedResponse<Response>>
    {
        private readonly IRepository<ValueEntity> _valueRepository;

        public AdminGetAllValuesHandler(IRepository<ValueEntity> valueRepository)
        {
            _valueRepository = valueRepository;
        }

        public async Task<PaginatedResponse<Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var query = _valueRepository
                .AsQueryable()
                .Include(v => v.Owner)
                .AsNoTracking();

            var totalCount = await query.CountAsync(cancellationToken);

            var sortBy = request.Pagination?.SortBy?.ToLower();
            var sortDirection = request.Pagination?.SortDirection?.ToLower();

            query = sortBy switch
            {
                "date" => sortDirection == "desc"
                    ? query.OrderByDescending(v => v.CreatedAt)
                    : query.OrderBy(v => v.CreatedAt),

                "key" => sortDirection == "desc"
                    ? query.OrderByDescending(v => v.Key)
                    : query.OrderBy(v => v.Key),

                "owner" => sortDirection == "desc"
                    ? query.OrderByDescending(v => v.Owner != null ? v.Owner.Username : "Global")
                    : query.OrderBy(v => v.Owner != null ? v.Owner.Username : "Global"),

                _ => query.OrderByDescending(v => v.CreatedAt)
            };

            var items = await query
                .Skip((request.Pagination!.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(t => new Response(
                    t.Key,
                    t.Id,
                    t.Translations.Count,
                    t.CreatedAt,
                    t.OwnerId,
                    t.Owner != null ? t.Owner.Username : "Global",
                    t.OwnerId == null ? "Global" : "User"
                ))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<Response>
            {
                Page = request.Pagination.Page,
                PageSize = request.Pagination.PageSize,
                TotalItems = totalCount,
                HasNextPage = request.Pagination.Page * request.Pagination.PageSize < totalCount,
                HasPreviousPage = request.Pagination.Page > 1,
                Items = items
            };
        }
    }
}
