using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands;

public abstract class DeleteCategory
{
    public sealed record Command(
        Guid Id) : IRequest;

    public class Handler(IRepository<CategoryEntity> categoryRepository) : IRequestHandler<Command>
    {
        public async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var categoryExists = await categoryRepository
                .AsQueryable()
                .FirstOrDefaultAsync(category => category.Id == request.Id, cancellationToken);

            if (categoryExists is null)
                throw new CategoryNotFoundException(request.Id);

            await categoryRepository.DeleteAsync([categoryExists]);
            await categoryRepository.SaveChangesAsync(cancellationToken);
        }
    }
}