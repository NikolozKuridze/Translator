using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public class CreateCategoryCommandHandler(ApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await CheckSiblings(request, cancellationToken);
        await CheckAncestors(request, cancellationToken);

        var category = new CategoryEntity(request.Value, request.Type, request.Order, request.ParentId);

        await context.Categories.AddAsync(category, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
    
    private async Task CheckAncestors(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var tempParentId = request.ParentId;

        while (tempParentId != null)
        {
            var parent = await context.Categories
                .FirstOrDefaultAsync(p => p.Id == tempParentId, cancellationToken);

            if (parent == null)
            {
                throw new InvalidOperationException($"Parent category with ID {tempParentId} not found");
            }

            if (request.Type.ToLower() == parent.Type.ToLower())
            {
                throw new InvalidOperationException(
                    $"Category type '{request.Type}' already exists in ancestor hierarchy");
            }

            tempParentId = parent.ParentId;
        }
    }
    private async Task CheckSiblings(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var parent = await context.Categories
            .Include(category => category.Children)
            .FirstOrDefaultAsync(p => p.Id == request.ParentId, cancellationToken);

        if (parent != null && parent.Children != null)
            if (parent.Children.Any(child => child.Type.ToLower() == request.Type.ToLower() &&
                                             child.Value.ToLower() == request.Value.ToLower()))
            {
                throw new InvalidOperationException(
                    "Sibling category already exists with same value and type");
            }
    }
}