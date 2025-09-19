using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.TemplatesAdmin.Queries;

public abstract class AdminGetAllTemplates
{
    public sealed record Command(
        PaginationRequest Pagination) : IRequest<PaginatedResponse<Response>>;

    public record Response(
        string TemplateName,
        Guid TemplateId,
        int ValueCount,
        Guid? OwnerId,
        string OwnerName,
        string OwnershipType,
        bool IsGlobal
    );

    public class Handler : IRequestHandler<Command, PaginatedResponse<Response>>
    {
        private readonly IRepository<TemplateEntity> _repository;

        public Handler(IRepository<TemplateEntity> repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedResponse<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var query = _repository
                .AsQueryable()
                .Include(t => t.Owner)
                .AsNoTracking();

            var sortBy = request.Pagination.SortBy?.ToLower();
            var sortDirection = request.Pagination.SortDirection;

            query = sortBy switch
            {
                "name" => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(t => t.Name)
                    : query.OrderBy(t => t.Name),

                "value" or "valuecount" => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(t => t.Values.Count)
                    : query.OrderBy(t => t.Values.Count),

                "owner" => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(t => t.Owner != null ? t.Owner.Username : "Global")
                    : query.OrderBy(t => t.Owner != null ? t.Owner.Username : "Global"),

                "type" or "ownership" => string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)
                    ? query.OrderByDescending(t => t.OwnerId == null ? "Global" : "User")
                    : query.OrderBy(t => t.OwnerId == null ? "Global" : "User"),

                _ => query
                    .OrderBy(t => t.OwnerId == null ? 0 : 1)
                    .ThenBy(t => t.Name)
            };

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((request.Pagination.Page - 1) * request.Pagination.PageSize)
                .Take(request.Pagination.PageSize)
                .Select(t => new Response(
                    t.Name,
                    t.Id,
                    t.Values.Count,
                    t.OwnerId,
                    t.Owner != null ? t.Owner.Username : "Global",
                    t.OwnerId == null ? "Global" : "User",
                    t.OwnerId == null
                ))
                .ToListAsync(cancellationToken);

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