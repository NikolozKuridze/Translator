using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Application.Exceptions;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler(IRepository<CategoryEntity> _categoryRepository) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var categoryExists = await _categoryRepository
            .AsQueryable()
            .FirstOrDefaultAsync(category => category.Id == request.Id);

        if (categoryExists is null)
            throw new CategoryNotFoundException(request.Id);

        await _categoryRepository.DeleteAsync([categoryExists]);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
    }
}