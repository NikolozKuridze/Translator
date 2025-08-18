using MediatR;
using Translator.Application.Helpers;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Commands.AddCategory;

public class CreateCategoryCommandHandler(
    IRepository<CategoryEntity> _categoryRepository,
    CategoryChecker categoryChecker) : IRequestHandler<CreateCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        await categoryChecker.CheckSiblingsAsync(request.Value, request.Type,
            request.ParentId, null, cancellationToken);
        await categoryChecker.CheckAncestorsAsync(request.Type, request.ParentId, cancellationToken);

        var category = new CategoryEntity(request.Value, request.Type, request.Order, request.ParentId);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _categoryRepository.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}