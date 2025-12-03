using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Domain.Pagination;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.ValuesAdmin.Queries;

public abstract class AdminSearchValue
{
    public sealed record Command(
        string? ValueKey,
        string? UserName,
        PaginationRequest PaginationRequest) : IRequest<PaginatedResponse<AdminGetAllValues.Response>>;
    
    public class AdminSearchValueHandler : IRequestHandler<Command, PaginatedResponse<AdminGetAllValues.Response>>
    {
        private readonly IRepository<Value> _valueRepository;

        public AdminSearchValueHandler(IRepository<Value> valueRepository)
        {
            _valueRepository = valueRepository;
        }

        public async Task<PaginatedResponse<AdminGetAllValues.Response>> Handle(AdminSearchValue.Command request,
            CancellationToken cancellationToken)
        {
            var query = _valueRepository
                .AsQueryable()
                .Include(v => v.Owner)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ValueKey))
            {
                var searchTerm = request.ValueKey.Trim().ToLower();
                query = query.Where(v =>
                    v.Key.ToLower().Contains(searchTerm) ||
                    v.Key.ToLower() == searchTerm);
            }

            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                var usernameSearchTerm = request.UserName.Trim().ToLower();
                query = query.Where(v =>
                    v.Owner != null && 
                    v.Owner.Username.ToLower().Contains(usernameSearchTerm));
            }

            var totalItems = await query.CountAsync(cancellationToken);

            var searchKey = request.ValueKey?.Trim().ToLower() ?? "";
            var searchUser = request.UserName?.Trim().ToLower() ?? "";
            
            query = query
                .OrderBy(v => v.Key.ToLower().StartsWith(searchKey) ? 0 : 1)
                .ThenBy(v => v.Owner != null && v.Owner.Username.ToLower().StartsWith(searchUser) ? 0 : 1)
                .ThenBy(v => v.OwnerId == null ? 0 : 1)
                .ThenByDescending(v => v.CreatedAt);

            var values = await query
                .Skip((request.PaginationRequest.Page - 1) * request.PaginationRequest.PageSize)
                .Take(request.PaginationRequest.PageSize)
                .Select(vk => new AdminGetAllValues.Response(
                    vk.Key,
                    vk.Id,
                    vk.Translations.Count,
                    vk.CreatedAt,
                    vk.OwnerId,
                    vk.Owner != null ? vk.Owner.Username : "Global",
                    vk.OwnerId == null ? "Global" : "User"
                ))
                .ToArrayAsync(cancellationToken);

            return new PaginatedResponse<AdminGetAllValues.Response>
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
