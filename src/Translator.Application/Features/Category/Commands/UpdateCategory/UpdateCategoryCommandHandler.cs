using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(
    IRepository<CategoryEntity> categoryRepository) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryToUpdate = await categoryRepository
            .AsQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        
        if (categoryToUpdate is null)
            throw new CategoryNotFoundException(request.Id);

        var proposedValue = request.Value ?? categoryToUpdate.Value;
        var proposedOrder = request.Order ?? categoryToUpdate.Order;

        categoryToUpdate.Value = proposedValue;
        categoryToUpdate.Order = proposedOrder;
        
        await categoryRepository.UpdateAsync(categoryToUpdate);
        await categoryRepository.SaveChangesAsync(cancellationToken);
    }
}