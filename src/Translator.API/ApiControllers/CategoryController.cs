using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.Category.Commands.AddCategory;
using Translator.Application.Features.Category.Commands.DeleteCategory;
using Translator.Application.Features.Category.Commands.UpdateCategory;
using Translator.Application.Features.Category.Queries.GetCategory;
using Translator.Application.Features.Category.Queries.GetRootCategories;


namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/category")]
public class CategoryController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryModel model)
    {
        var command = new CreateCategoryCommand(model.Value.ToLower().Trim(), model.Type.ToLower().Trim(), model.Order, model.ParentId);
        var categoryId = await mediator.Send(command);
        return Ok(categoryId);
    }
    
    [HttpGet("{categoryId}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategory(Guid categoryId)
    {
        var category = await mediator.Send(new GetCategoryQuery(categoryId));
        
        return Ok(category);
    }
    
    [HttpGet("roots")]
    public async Task<ActionResult<IEnumerable<RootCategoryDto>>> GetRootCategories()
    {
        var categories = await mediator.Send(new GetRootCategoriesQuery());
        
        return Ok(categories);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCategory(DeleteCategoryModel model)
    {
        var command = new DeleteCategoryCommand(model.Id);
        await mediator.Send(command);
        
        return  NoContent();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateCategory(UpdateCategoryModel model)
    {
        var command = new UpdateCategoryCommand(model.Id, model.Value?.ToLower().Trim(), model.Order);
        await mediator.Send(command);
        
        return NoContent();
    }
}