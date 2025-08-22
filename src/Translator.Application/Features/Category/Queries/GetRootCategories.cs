using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Queries;

public abstract class GetRootCategories
{
    public sealed record Query : IRequest<IEnumerable<Response>>;

    public sealed record Response(
        Guid Id,
        string Value,
        string TypeName,
        int? Order);

    public class Handler(IRepository<CategoryEntity> categoryRepository) : IRequestHandler<Query, IEnumerable<Response>>
    {
        public async Task<IEnumerable<Response>> Handle(Query request, CancellationToken cancellationToken)
        {
            var categories = await categoryRepository
                .AsQueryable()
                .Where(c => c.ParentId == null)
                .Include(c => c.Type)
                .OrderBy(c => c.Order)
                .ToListAsync(cancellationToken);

            return await MapToResponse(categories);
        }

        private static async Task<IEnumerable<Response>> MapToResponse(IEnumerable<CategoryEntity> categories)
        {
            return await Task.FromResult(categories.Select(c
                => new Response(
                    c.Id,
                    c.Value,
                    c.Type.Name,
                    c.Order)).ToList());
        }
    }
}