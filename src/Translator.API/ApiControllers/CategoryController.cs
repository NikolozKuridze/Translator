using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
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
    public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryContract contract)
    {
        var command = new CreateCategoryCommand(contract.Value.ToLower().Trim(), contract.Type.ToLower().Trim(), contract.Order, contract.ParentId);
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
    public async Task<ActionResult> DeleteCategory(DeleteCategoryContract contract)
    {
        var command = new DeleteCategoryCommand(contract.Id);
        await mediator.Send(command);
        
        return  NoContent();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateCategory(UpdateCategoryContract contract)
    {
        var command = new UpdateCategoryCommand(contract.Id, contract.Value?.ToLower().Trim(), contract.Order);
        await mediator.Send(command);
        
        return NoContent();
    }
}