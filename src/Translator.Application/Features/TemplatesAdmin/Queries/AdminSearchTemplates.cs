using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.TemplatesAdmin.Queries;

public abstract class AdminSearchTemplate
{
    public sealed record Command(
        string? TemplateName,
        string? OwnerName,
        string? OwnershipType,
        PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<AdminGetAllTemplates.Response>>;

    public class Handler : IRequestHandler<Command, PaginatedResponse<AdminGetAllTemplates.Response>>
    {
        private readonly IRepository<TemplateEntity> _templateRepository;

        public Handler(IRepository<TemplateEntity> templateRepository)
        {
            _templateRepository = templateRepository;
        }

        public async Task<PaginatedResponse<AdminGetAllTemplates.Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            if (request.PaginationRequest == null)
                throw new ArgumentNullException(nameof(request.PaginationRequest), "Pagination request cannot be null");

            var query = _templateRepository
                .AsQueryable()
                .Include(t => t.Owner)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.TemplateName))
            {
                var templateSearchTerm = request.TemplateName.Trim().ToLower();
                query = query.Where(t =>
                    t.Name.ToLower().Contains(templateSearchTerm) ||
                    t.Name.ToLower() == templateSearchTerm);
            }

            if (!string.IsNullOrWhiteSpace(request.OwnerName))
            {
                var ownerSearchTerm = request.OwnerName.Trim().ToLower();
                query = query.Where(t =>
                    (t.Owner != null && t.Owner.Username.ToLower().Contains(ownerSearchTerm)) ||
                    (t.Owner == null && "global".Contains(ownerSearchTerm)));
            }

            if (!string.IsNullOrWhiteSpace(request.OwnershipType))
            {
                var ownershipFilter = request.OwnershipType.Trim().ToLower();
                query = ownershipFilter switch
                {
                    "global" => query.Where(t => t.OwnerId == null),
                    "user" => query.Where(t => t.OwnerId != null),
                    _ => query
                };
            }

            var sortBy = request.PaginationRequest.SortBy?.ToLower();
            var sortDirection = request.PaginationRequest.SortDirection;

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
                    .OrderBy(t => string.IsNullOrEmpty(request.TemplateName) ? 0 :
                        t.Name.ToLower().StartsWith(request.TemplateName.ToLower()) ? 0 : 1)
                    .ThenBy(t => t.OwnerId == null ? 0 : 1)
                    .ThenBy(t => t.Name)
            };

            var totalItems = await query.CountAsync(cancellationToken);

            var templates = await query
                .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .Select(tk => new AdminGetAllTemplates.Response(
                    tk.Name,
                    tk.Id,
                    tk.Values.Count,
                    tk.OwnerId,
                    tk.Owner != null ? tk.Owner.Username : "Global",
                    tk.OwnerId == null ? "Global" : "User",
                    tk.OwnerId == null
                ))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<AdminGetAllTemplates.Response>
            {
                Page = request.PaginationRequest.Page,
                PageSize = request.PaginationRequest.PageSize,
                TotalItems = totalItems,
                HasNextPage = request.PaginationRequest.Page * request.PaginationRequest.PageSize < totalItems,
                HasPreviousPage = request.PaginationRequest.Page > 1,
                Items = templates
            };
        }
    }
}