using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetRootCategories;

public class GetRootCategoriesQueryHandler(IRepository<CategoryEntity> categoryRepository)
    : IRequestHandler<GetRootCategoriesQuery, IEnumerable<RootCategoryDto>>
{
    public async Task<IEnumerable<RootCategoryDto>> Handle(GetRootCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository
            .AsQueryable()
            .Where(c => c.ParentId == null)
            .Include(c => c.Type)
            .OrderBy(c => c.Order)
            .ToListAsync(cancellationToken);

        return await MapToRootcategoryDto(categories);
    }

    private static async Task<List<RootCategoryDto>> MapToRootcategoryDto(IEnumerable<CategoryEntity> categories)
    {
        var textInfo = CultureInfo.CurrentCulture.TextInfo;
        
        return await Task.FromResult(categories.Select(c 
            =>  new RootCategoryDto(
                c.Id,
                textInfo.ToTitleCase(c.Value),
                textInfo.ToTitleCase(c.Type.Name),
                c.Order)).ToList());
    }
}

public record RootCategoryDto(
    Guid Id,
    string Value,
    string TypeName,
    int? Order);