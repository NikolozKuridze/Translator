using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Queries;

public abstract class GetCategoryTree
{
    public sealed record Query(Guid Id) : IRequest<Response>;

    public sealed record Response(
        Guid Id,
        string Value,
        string TypeName,
        int? Order,
        Guid? ParentId,
        List<Response> Children);

    public class Handler(IRepository<CategoryEntity> categoryRepository) : IRequestHandler<Query, Response>
    {
        public async Task<Response> Handle(Query request, CancellationToken cancellationToken)
        {
            var category = await categoryRepository
                .AsQueryable()
                .Include(c => c.Type)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
                throw new CategoryNotFoundException(request.Id);

            return await MapToResponseWithChildren(category, cancellationToken);
        }

        private async Task<Response> MapToResponseWithChildren(CategoryEntity category,
            CancellationToken cancellationToken)
        {
            var children = await categoryRepository
                .AsQueryable()
                .Include(c => c.Type)
                .Where(c => c.ParentId == category.Id)
                .OrderBy(c => c.Order)
                .ToListAsync(cancellationToken);

            var childDtos = new List<Response>();
            foreach (var child in children)
            {
                var childDto = await MapToResponseWithChildren(child, cancellationToken);
                childDtos.Add(childDto);
            }

            return new Response(
                category.Id,
                category.Value,
                category.Type.Name,
                category.Order,
                category.ParentId,
                childDtos
            );
        }
    }
}