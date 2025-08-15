using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategory;

public class GetCategoryByIdQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetCategoryByIdQuery, CategoryReadDto?>
{
    public async Task<CategoryReadDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        return category != null ? MapToReadDto(category) : null;
    }

    private CategoryReadDto MapToReadDto(CategoryEntity category)
    {
        var dto = new CategoryReadDto(category.Id, category.Value, category.Type, category.Order, category.ParentId );
        
        if (category.Children is not null)
        {
            dto.Children = category.Children.Select(c => MapToReadDto(c)).ToList();
        }

        return dto;
    }
}

public class CategoryReadDto(Guid id, string value, string type, int? order, Guid? parentId)
{
    public Guid Id { get; set; } = id;
    public string Value { get; set; } = value;
    public string Type { get; set; } = type;
    public int? Order { get; set; } = order;
    public Guid? ParentId { get; set; } = parentId;
    public List<CategoryReadDto> Children { get; set; } = [];
}