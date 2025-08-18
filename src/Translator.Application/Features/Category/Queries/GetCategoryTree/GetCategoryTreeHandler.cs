using MediatR;
using Microsoft.EntityFrameworkCore;
using Translator.Infrastructure.Database.Postgres;
using CategoryEntity = Translator.Domain.DataModels.Category;

namespace Translator.Application.Features.Category.Queries.GetCategoryTree;

public class GetCategoryTreeHandler : IRequestHandler<GetCategoryTreeCommand, IEnumerable<CategoryEntity>>
{
    private readonly ApplicationDbContext _context;

    public GetCategoryTreeHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryEntity>> Handle(GetCategoryTreeCommand request, CancellationToken cancellationToken)
    {
        var tree = await GetSubtreeAsync(request.Id);
        return tree;
    }
    
    public async Task<List<CategoryEntity>> GetSubtreeAsync(Guid parentId)
    {
        var subtree = await _context.Categories
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
    
    private async Task<List<CategoryEntity>> GetTreeAsync()
    {
        var all = await _context.Categories
            .OrderBy(c => c.Order)
            .ToListAsync();

        foreach (var cat in all)
        {
            cat.Children = new List<CategoryEntity>();
            cat.Parent = null;
        }

        var lookup = all.ToDictionary(c => c.Id);
        List<CategoryEntity> roots = new();

        foreach (var cat in all)
        {
            if (cat.ParentId is Guid pid && lookup.TryGetValue(pid, out var parent))
            {
                parent.Children.Add(cat);
                cat.Parent = parent;
            }
            else
                roots.Add(cat);
        }
        return roots;
    }
    
    List<CategoryEntity> BuildTree(List<CategoryEntity> flat)
    {
        foreach (var cat in flat)
        {
            cat.Children = new List<CategoryEntity>();
            cat.Parent = null; 
        }
        var lookup = flat.ToDictionary(c => c.Id);
        List<CategoryEntity> roots = new();

        foreach (var cat in flat)
        {
            if (cat.ParentId is Guid pid && lookup.TryGetValue(pid, out var parent))
                parent.Children.Add(cat);
            else
                roots.Add(cat);
        }

        return roots;
    }
    private GetCategoryTreeResponse MapToReadDto(CategoryEntity category)
    {
        var dto = new GetCategoryTreeResponse(category.Id, category.Value, category.Type, category.Order, category.ParentId);
        
        if (category.Children is not null)
            dto.Children = category.Children.Select(c => MapToReadDto(c)).ToList();

        return dto;
    }
}

public record GetCategoryTreeResponse(Guid Id, string Value, string Type, int? Order, Guid? ParentId)
{
    public List<GetCategoryTreeResponse> Children { get; set; } = [];
}