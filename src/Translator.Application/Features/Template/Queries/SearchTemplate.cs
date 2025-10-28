using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries;

public abstract class SearchTemplate
{
    public sealed record Command(
        string TemplateName,
        PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<GetAllTemplates.Response>>;

    public class Handler : IRequestHandler<Command, PaginatedResponse<GetAllTemplates.Response>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<TemplateEntity> _templateRepository;
        private readonly IRepository<User> _userRepository;

        public Handler(
            ICurrentUserService currentUserService,
            IRepository<TemplateEntity> templateRepository,
            IRepository<User> userRepository)
        {
            _currentUserService = currentUserService;
            _templateRepository = templateRepository;
            _userRepository = userRepository;
        }

        public async Task<PaginatedResponse<GetAllTemplates.Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            if (request.PaginationRequest == null)
                throw new ArgumentNullException(nameof(request.PaginationRequest), "Pagination request cannot be null");

            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var query = _templateRepository
                .AsQueryable()
                .Include(t => t.Owner)
                .Where(t => t.OwnerId == userId.Value || t.OwnerId == null)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.TemplateName))
            {
                var searchTerm = request.TemplateName.Trim();
                query = query.Where(t =>
                    t.Name.Contains(searchTerm) ||
                    (t.Owner != null && t.Owner.Username.Contains(searchTerm))
                );
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
                .Select(tk => new GetAllTemplates.Response(
                    tk.Name,
                    tk.Id,
                    tk.Values.Count,
                    tk.OwnerId,
                    tk.Owner != null ? tk.Owner.Username : "Global",
                    tk.OwnerId == null ? "Global" : "User",
                    tk.OwnerId == userId.Value
                ))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<GetAllTemplates.Response>
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