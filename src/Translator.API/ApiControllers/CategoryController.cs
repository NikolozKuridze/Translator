using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Category.Commands;
using Translator.Application.Features.Category.Queries;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/category")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> CreateCategory(CreateCategoryModel model)
    {
        var command = new AddCategory.Command(model.Value, model.Type, model.Order,
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

    [HttpGet("roots")]
    public async Task<IResult> GetRootCategories()
    {
        var categories = await mediator.Send(new GetRootCategories.Query());

        return Results.Ok(categories);
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
        var command = new UpdateCategory.Command(model.Id, model.Value?.ToLower().Trim(), model.Order);

        await mediator.Send(command);

        return Results.Ok();
    }
}