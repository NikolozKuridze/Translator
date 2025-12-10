using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Users.Queries;

public abstract class SearchUsers
{
    public sealed record Query(
        string? UserName,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PaginatedResponse<Response>>;

    public sealed record Response(
        Guid UserId,
        string UserName,
        string SecretKey
    );

    public class Handler(
        IRepository<User> userRepository
    ) : IRequestHandler<Query, PaginatedResponse<Response>>
    {
        public async Task<PaginatedResponse<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = userRepository.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var searchTerm = request.UserName.ToLower().Trim();
                query = query.Where(u => u.Username.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageNumber = Math.Max(1, request.PageNumber);
            var effectivePageSize = Math.Min(100, request.PageSize);

            var users = await query
                .Skip((pageNumber - 1) * effectivePageSize)
                .Take(effectivePageSize)
                .Select(u => new Response(
                    u.Id,
                    u.Username,
                    u.SecretKey
                ))
                .ToListAsync(cancellationToken);

            return new PaginatedResponse<Response>
            {
                Page = pageNumber,
                PageSize = effectivePageSize,
                TotalItems = totalCount,
                Items = users,
                HasNextPage = pageNumber < (int)Math.Ceiling((double)totalCount / effectivePageSize),
                HasPreviousPage = pageNumber > 1
            };
        }
    }
}
