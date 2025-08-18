using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetRootCategories;

public class GetRootCategoriesQueryHandler(IRepository<CategoryEntity> _categoryRepository)
    : IRequestHandler<GetRootCategoriesQuery, IEnumerable<RootCategoryDto>>
{
    public async Task<IEnumerable<RootCategoryDto>> Handle(GetRootCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _categoryRepository
            .AsQueryable()
            .Where(c => c.ParentId == null)
            .ToListAsync(cancellationToken);

        return await MapToRootcategoryDto(categories);
    }

    private async Task<List<RootCategoryDto>> MapToRootcategoryDto(IEnumerable<CategoryEntity> categories)
    {
        return await Task.FromResult(categories.Select(c =>  new RootCategoryDto(c.Id, c.Value, c.Type, c.Order)).ToList());
    }
}

public record RootCategoryDto(Guid Id, string Value, string Type, int? Order);