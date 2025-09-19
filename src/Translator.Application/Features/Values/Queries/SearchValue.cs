using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Contracts.Infrastructure;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.Values.Queries;

public abstract class SearchValue
{
    public sealed record Command(
        string ValueKey,
        PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<GetAllValues.Response>>;

    public class SearchValueHandler : IRequestHandler<Command, PaginatedResponse<GetAllValues.Response>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Value> _valueRepository;

        public SearchValueHandler(
            IRepository<Value> valueRepository,
            IRepository<User> userRepository,
            ICurrentUserService currentUserService)
        {
            _valueRepository = valueRepository;
            _userRepository = userRepository;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedResponse<GetAllValues.Response>> Handle(Command request,
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

            if (!string.IsNullOrWhiteSpace(request.ValueKey))
            {
                var searchTerm = request.ValueKey.Trim().ToLower();
                query = query.Where(v =>
                    v.Key.ToLower().Contains(searchTerm) ||
                    v.Key.ToLower() == searchTerm);
            }

            var totalItems = await query.CountAsync(cancellationToken);

            query = query
                .OrderBy(v => v.Key.ToLower().StartsWith(request.ValueKey.ToLower() ?? "") ? 0 : 1)
                .ThenByDescending(v => v.CreatedAt);

            var values = await query
                .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .Select(vk => new GetAllValues.Response(
                    vk.Key,
                    vk.Id,
                    vk.Translations.Count,
                    vk.CreatedAt,
                    vk.OwnerId == null ? "Global" : "User",
                    vk.OwnerId == userId.Value
                ))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<GetAllValues.Response>
            {
                Page = request.PaginationRequest.Page,
                PageSize = request.PaginationRequest.PageSize,
                TotalItems = totalItems,
                HasNextPage = request.PaginationRequest.Page * request.PaginationRequest.PageSize < totalItems,
                HasPreviousPage = request.PaginationRequest.Page > 1,
                Items = values
            };
        }
    }
}