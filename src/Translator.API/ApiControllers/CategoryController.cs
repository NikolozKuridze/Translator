using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Category.Commands.AddCategory;

namespace Translator.API.ApiControllers;

[ApiController]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoryController(IMediator mediator)
        => _mediator = mediator;
    
    [HttpPost("api/category/add")]
    public async Task<ActionResult<Guid>> CreateCategory(CreateCategoryCommand command)
    {
        var categoryId = await _mediator.Send(command);
        return Ok(categoryId);
    }
}