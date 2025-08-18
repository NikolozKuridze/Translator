using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public class CreateCategoryCommandHandler(IRepository<CategoryEntity> _categoryRepository) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await CheckSiblings(request, cancellationToken);
        await CheckAncestors(request, cancellationToken);

        var category = new CategoryEntity(request.Value, request.Type, request.Order, request.ParentId);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category.Id;
    }

    private async Task CheckAncestors(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tempParentId = request.ParentId;

        while (tempParentId != null)
        {
            var parent = await _categoryRepository.AsQueryable()    
                .FirstOrDefaultAsync(p => p.Id == tempParentId, cancellationToken);

            if (parent == null)
                throw new CategoryNotFoundException(tempParentId);

            if (request.Type.ToLower() == parent.Type.ToLower())
                throw new CategoryAlreadyExistsException();

            tempParentId = parent.ParentId;
        }
    }

    private async Task CheckSiblings(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var parent = await _categoryRepository.AsQueryable()
            .Include(category => category.Children)
            .FirstOrDefaultAsync(p => p.Id == request.ParentId, cancellationToken);

        if (parent != null && parent.Children != null)
            if (parent.Children.Any(child => child.Type.ToLower() == request.Type.ToLower() &&
                                             child.Value.ToLower() == request.Value.ToLower()))
            {
                throw new CategoryAlreadyExistsException();
            }
    }
}