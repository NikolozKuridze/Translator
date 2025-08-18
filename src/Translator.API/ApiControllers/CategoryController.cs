using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Category.Commands.AddCategory;
using Translator.Application.Features.Category.Queries.GetCategory;
using Translator.Application.Features.Category.Queries.GetCategoryTree;

namespace Translator.API.ApiControllers;

[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("api/category/add")]
    public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryContract contract)
    {
        var command = new CreateCategoryCommand(contract.Value.ToLower().Trim(), contract.Type.ToLower().Trim(), contract.Order, contract.ParentId);
        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }
    
    
    [HttpGet("api/category/{categoryId}")]
    public async Task<ActionResult<CategoryReadDto>> GetCategory(Guid categoryId)
    {
        var category = await _mediator.Send(new GetCategoryQuery(categoryId));
        
        return Ok(category);
    }
    [HttpGet("api/category/get-all/{categoryId}")]
    public async Task<ActionResult<CategoryReadDto>> GetAllCategories(Guid categoryId)
    {
        var category = await _mediator.Send(new GetCategoryTreeCommand(categoryId));
        
        return Ok(category);
    }
}