using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Category.Commands;
using Translator.Application.Features.Category.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/category")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateCategory(CreateCategoryModel model)
    {
        var command = new AddCategory.Command(
            model.Value,
            model.Type,
            model.Metadata,
            model.Shortcode,
            model.Order,
            model.ParentId);

        var result = await mediator.Send(command);

        return Results.Ok(result);
    }

    [HttpGet("{categoryId}")]
    public async Task<IResult> GetCategory(Guid categoryId)
    {
        var category = await mediator.Send(new GetCategoryTree.Query(categoryId));

        return Results.Ok(category);
    }

    [HttpGet("root-catgories/{pageNumber}/{pageSize}")]
    public async Task<IResult> GetRootCategories(int pageNumber = 1, int pageSize = 10)
    {
        var query = new GetRootCategories.Query(new PaginationRequest(pageNumber, pageSize, null, null, null, null));
        var result = await mediator.Send(query);

        return Results.Ok(result);
    }

    [HttpGet("search-categories")]
    public async Task<IResult> SearchRootCategories(string categoryName, int pageNumber, int pageSize)
    {
        var command = new SearchRootCategories.Query(
            categoryName,
            new PaginationRequest(pageNumber, pageSize, null, null, null, null));
        
        var result = await mediator.Send(command);
        
        return Results.Ok(result);
    }

    [HttpDelete]
    public async Task<IResult> DeleteCategory(DeleteCategoryModel model)
    {
        var command = new DeleteCategory.Command(model.Id);

        await mediator.Send(command);

        return Results.Ok();
    }

    [HttpPut]
    public async Task<IResult> UpdateCategory(UpdateCategoryModel model)
    {
        var command = new UpdateCategory.Command(
            model.Id,
            model.Value?.ToLower().Trim(),
            model.Metadata,
            model.Shortcode?.ToLower().Trim(),
            model.Order);

        await mediator.Send(command);

        return Results.Ok();
    }
}