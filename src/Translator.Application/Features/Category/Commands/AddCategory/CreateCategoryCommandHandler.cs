using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public class CreateCategoryCommandHandler(
    IRepository<CategoryEntity> categoryRepository,
    IRepository<CategoryType> typeRepository) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await categoryRepository.AsQueryable()
                .Include(p => p.Type)
                .FirstOrDefaultAsync(p => p.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
                throw new CategoryNotFoundException(request.ParentId.Value);

            if (parent.Type.Name.Equals(request.TypeName, StringComparison.OrdinalIgnoreCase))
                throw new SameTypeException();
        }
        
        var typeExists = await typeRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.Name == request.TypeName,cancellationToken);

        if (typeExists is null)
        {
            throw new TypeNotFoundException(request.TypeName);
        }
        
        var category = new CategoryEntity(request.Value, typeExists.Id, request.Order, request.ParentId);
        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}