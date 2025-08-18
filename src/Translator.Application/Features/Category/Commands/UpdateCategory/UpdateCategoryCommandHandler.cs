using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler(IRepository<CategoryEntity> _categoryRepository, CategoryChecker categoryChecker) : IRequestHandler<UpdateCategoryCommand>
{
    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryToUpdate = await _categoryRepository
            .AsQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        
        if (categoryToUpdate is null)
            throw new CategoryNotFoundException(request.Id);

        var proposedValue = request.Value ?? categoryToUpdate.Value;
        var proposedType = request.Type ?? categoryToUpdate.Type;
        var proposedOrder = request.Order ?? categoryToUpdate.Order;
        var proposedParentId = request.ParentId ?? categoryToUpdate.ParentId;

        await CheckChildrenAsync(proposedType, request.Id, cancellationToken);
        await categoryChecker.CheckSiblingsAsync(proposedValue, proposedType, proposedParentId, request.Id, cancellationToken);
        await categoryChecker.CheckAncestorsAsync(proposedType, proposedParentId, cancellationToken);
        
        categoryToUpdate.Value = proposedValue;
        categoryToUpdate.Type = proposedType;
        categoryToUpdate.Order = proposedOrder;
        categoryToUpdate.ParentId = proposedParentId;
        
        await _categoryRepository.UpdateAsync(categoryToUpdate);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckChildrenAsync(string proposedType, Guid categoryId, CancellationToken cancellationToken)
    {
        var children = await _categoryRepository
            .AsQueryable()
            .Where(c => c.ParentId == categoryId)
            .ToArrayAsync(cancellationToken);

        foreach (var child in children)
        {
            if (child.Type == proposedType)
                throw new CategoryAlreadyExistsException();
            
            await CheckChildrenAsync(proposedType, child.Id, cancellationToken);
        }
    }
}