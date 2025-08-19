using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategoryTree;

public class GetCategoryTreeHandler(ApplicationDbContext context)
    : IRequestHandler<GetCategoryTreeCommand, IEnumerable<GetCategoryTreeResponse>>
{
    public async Task<IEnumerable<GetCategoryTreeResponse>> Handle(GetCategoryTreeCommand request, CancellationToken cancellationToken)
    {
        var tree = await GetSubtreeAsync(request.Id);

        return tree.Select(MapToReadDto);
    }

    private async Task<List<CategoryEntity>> GetSubtreeAsync(Guid parentId)
    {
        var subtree = await context.Categories
            .FromSql($@"
                WITH RECURSIVE tree AS (
                SELECT * FROM ""categories"" WHERE ""id"" = {parentId}
                UNION ALL
                SELECT c.* FROM ""categories"" c
                JOIN tree t ON c.""parent_id"" = t.""id""
                )
                SELECT * FROM tree
                ")
            .OrderBy(c => c.Order)
            .ToListAsync();

        return BuildTree(subtree);
    }

    private static List<CategoryEntity> BuildTree(List<CategoryEntity> flat)
    {
        foreach (var cat in flat)
        {
            cat.Parent = null;
        }

        var lookup = flat.ToDictionary(c => c.Id);
        List<CategoryEntity> roots = new();

        foreach (var category in flat)
        {
            if (category.ParentId is { } pid && lookup.TryGetValue(pid, out var parent))
            {
                category.Parent = parent;
            }
            else
                roots.Add(category);
        }

        return roots;
    }
    private GetCategoryTreeResponse MapToReadDto(CategoryEntity category)
    {
        var dto = new GetCategoryTreeResponse(
            category.Id, 
            category.Value, 
            category.Type, 
            category.Order
        );
        
        if (category.Children is not null)
            dto.Children = category.Children.Select(MapToReadDto).ToList();
        
        return dto;
    }
}

public record GetCategoryTreeResponse(Guid Id, string Value, string Type, int? Order)
{
    public List<GetCategoryTreeResponse> Children { get; set; } = [];
}