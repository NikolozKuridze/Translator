using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;
using ValueEntity = Translator.Domain.Entities.Value;

namespace Translator.Application.Features.Values.Queries;

public abstract class GetAllValues
{
    public sealed record Command(
        PaginationRequest Pagination) :
        IRequest<PaginatedResponse<Response>>;

    public sealed record Response(
        string Key,
        Guid ValueId,
        int TranslationsCount,
        DateTimeOffset CreatedAt,
        string OwnershipType,
        bool IsOwnedByCurrentUser
    );

    public class GetAllValuesHandler : IRequestHandler<Command, PaginatedResponse<Response>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<ValueEntity> _valueRepository;

        public GetAllValuesHandler(
            IRepository<ValueEntity> valueRepository,
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _valueRepository = valueRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedResponse<Response>> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUserService.GetCurrentUserId();
            if (!userId.HasValue)
                throw new UnauthorizedAccessException("User authentication required");

            var user = await _userRepository
                .Where(u => u.Id == userId.Value)
                .SingleOrDefaultAsync(cancellationToken);

            if (user == null)
                throw new UserNotFoundException(userId.Value);

            var query = _valueRepository
                .AsQueryable()
                .Where(v => v.OwnerId == userId.Value || v.OwnerId == null)
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
                    ? query.OrderByDescending(v => v.OwnerId.HasValue ? "User" : "Global")
                    : query.OrderBy(v => v.OwnerId.HasValue ? "User" : "Global"),

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
                    t.OwnerId == null ? "Global" : "User",
                    t.OwnerId == userId.Value
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