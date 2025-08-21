using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;

public class DeleteCategoryTypeCommandHandler(IRepository<CategoryType> typeRepository) : IRequestHandler<DeleteCategoryTypeCommand>
{
    public async Task Handle(DeleteCategoryTypeCommand request, CancellationToken cancellationToken)
    {
        var typesToDelete = await typeRepository.AsQueryable()
            .Where(t => request.TypeNames.Contains(t.Name))
            .ToArrayAsync(cancellationToken);

        var notFoundTypes = request.TypeNames.Except(typesToDelete.Select(t => t.Name)).ToList();
        if (notFoundTypes.Count != 0)
            throw new TypeNotFoundException($"Category types not found: {string.Join(", ", notFoundTypes)}");

        await typeRepository.DeleteAsync(typesToDelete);
        await typeRepository.SaveChangesAsync(cancellationToken);
    }
}