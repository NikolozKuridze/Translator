using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using TemplateEntity = Translator.Domain.Entities.Template;

namespace Translator.Application.Features.Template.Queries;

public abstract class GetAllTemplates
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
        bool IsOwnedByCurrentUser
    );

    public class Handler : IRequestHandler<Command, PaginatedResponse<Response>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<TemplateEntity> _repository;
        private readonly IRepository<User> _userRepository;

        public Handler(
            ICurrentUserService currentUserService,
            IRepository<TemplateEntity> repository,
            IRepository<User> userRepository)
        {
            _currentUserService = currentUserService;
            _repository = repository;
            _userRepository = userRepository;
        }

        public async Task<PaginatedResponse<Response>> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var query = _repository
                .AsQueryable()
                .Include(t => t.Owner)
                .Where(t => t.OwnerId == userId.Value || t.OwnerId == null)
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

                _ => query.OrderBy(t => t.Name)
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
                    t.OwnerId == userId.Value
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