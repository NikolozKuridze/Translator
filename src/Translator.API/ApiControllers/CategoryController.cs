using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Category.Commands.AddCategory;
using Translator.Application.Features.Category.Commands.DeleteCategory;
using Translator.Application.Features.Category.Commands.UpdateCategory;
using Translator.Application.Features.Category.Queries.GetCategory;
using Translator.Application.Features.Category.Queries.GetCategoryTree;
using Translator.Application.Features.Category.Queries.GetRootCategories;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/category")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("add")]
    public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryContract contract)
    {
        var command = new CreateCategoryCommand(contract.Value.ToLower().Trim(), contract.Type.ToLower().Trim(), contract.Order, contract.ParentId);
        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }
    
    [HttpGet("{categoryId}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategory(Guid categoryId)
    {
        var category = await _mediator.Send(new GetCategoryQuery(categoryId));
        
        return Ok(category);
    }
    [HttpGet("get-tree/{categoryId}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategoryTree(Guid categoryId)
    {
        var category = await _mediator.Send(new GetCategoryTreeCommand(categoryId));
        
        return Ok(category);
    }
    
    [HttpGet("roots")]
    public async Task<ActionResult<IEnumerable<RootCategoryDto>>> GetRootCategories()
    {
        var categories = await _mediator.Send(new GetRootCategoriesQuery());
        
        return Ok(categories);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteCategory(DeleteCategoryContract contract)
    {
        var command = new DeleteCategoryCommand(contract.Id);
        await _mediator.Send(command);
        
        return  NoContent();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateCategory(UpdateCategoryContract contract)
    {
        var command = new UpdateCategoryCommand(contract.Id, contract.Type?.ToLower().Trim(), contract.Value?.ToLower().Trim(), contract.Order, contract.ParentId);
        await _mediator.Send(command);
        
        return NoContent();
    }
}