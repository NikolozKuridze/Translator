using System.ComponentModel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Application.Helpers;
using Translator.Domain.DataModels;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public class CreateCategoryCommandHandler(
    IRepository<CategoryEntity> _categoryRepository,
    IRepository<CategoryType> _typeRepository) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (request.ParentId.HasValue)
        {
            var parent = await _categoryRepository.AsQueryable()
                .Include(p => p.Type)
                .FirstOrDefaultAsync(p => p.Id == request.ParentId.Value, cancellationToken);

            if (parent is null)
                throw new CategoryNotFoundException(request.ParentId.Value);

            if (parent.Type.Type.Equals(request.Type, StringComparison.OrdinalIgnoreCase))
                throw new CategoryAlreadyExistsException();
        }

        var typeExists = await _typeRepository.AsQueryable()
            .FirstOrDefaultAsync(t => t.Type.ToLower() == request.Type.ToLower(),
                cancellationToken);

        // if (typeExists is null)
        // {
        //     var newType = new CategoryType(request.Type);
        //     await _typeRepository.AddAsync(newType, cancellationToken);
        //     await _typeRepository.SaveChangesAsync(cancellationToken);
        //     
        //     var category = new CategoryEntity(request.Value, newType.Id, request.Order, request.ParentId);
        //     await _categoryRepository.AddAsync(category, cancellationToken);
        //     await _categoryRepository.SaveChangesAsync(cancellationToken);
        //
        //     return category.Id;
        // }
        // else
        // {
        //     var category = new CategoryEntity(request.Value, typeExists.Id, request.Order, request.ParentId);
        //     await _categoryRepository.AddAsync(category, cancellationToken);
        //     await _categoryRepository.SaveChangesAsync(cancellationToken);
        //
        //     return category.Id;
        // }
    }
}