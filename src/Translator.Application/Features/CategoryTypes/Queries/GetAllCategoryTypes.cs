using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Queries;

public abstract class GetAllCategoryTypes
{
    public sealed record Command : IRequest<Response>;

    public sealed record Response(
        IEnumerable<string> TypeNames);

    public class Handler(IRepository<CategoryType> typeRepository) : IRequestHandler<Command, Response>
    {
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var types = await typeRepository.AsQueryable()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            return new Response(types.Select(ct => ct.Name));
        }
    }
}