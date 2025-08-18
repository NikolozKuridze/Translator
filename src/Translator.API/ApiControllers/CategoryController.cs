using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.Category.Commands.AddCategory;
using Translator.Application.Features.Category.Commands.DeleteCategory;
using Translator.Application.Features.Category.Queries.GetCategory;

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
        var category = await _mediator.Send(new GetCategoryByIdQuery(categoryId));
        
        return Ok(category);
    }

    [HttpDelete("api/category")]
    public async Task<ActionResult> DeleteCategory(DeleteCategoryContract contract)
    {
        var command = new DeleteCategoryCommand(contract.Id);
        await _mediator.Send(command);
        
        return  NoContent();
    }
}