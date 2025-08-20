using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.Entities.Category;

namespace Translator.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(IRepository<CategoryEntity> _categoryRepository) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _categoryRepository
            .AsQueryable()
            .FirstOrDefaultAsync(category => category.Id == request.Id, cancellationToken: cancellationToken);

        if (categoryExists is null)
            throw new CategoryNotFoundException(request.Id);

        await _categoryRepository.DeleteAsync([categoryExists]);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }
}