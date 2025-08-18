using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.Repository;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategory;

public class GetCategoryHandler(IRepository<CategoryEntity> _categoryRepository)
    : IRequestHandler<GetCategoryQuery, CategoryReadDto?>
{
    public async Task<CategoryReadDto?> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository
            .AsQueryable()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return category != null ? MapToReadDto(category) : null;
    }

    private CategoryReadDto MapToReadDto(CategoryEntity category)
    {
        var dto = new CategoryReadDto(category.Id, category.Value, category.Type, category.Order, category.ParentId);
        
        if (category.Children is not null)
            dto.Children = category.Children.Select(c => MapToReadDto(c)).ToList();

        return dto;
    }
}

public record CategoryReadDto(Guid Id, string Value, string Type, int? Order, Guid? ParentId)
{
    public List<CategoryReadDto> Children { get; set; } = [];
}