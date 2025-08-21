using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.CategoryTypes.Commands.CreateBulkCategoryType;
using Translator.Application.Features.CategoryTypes.Commands.CreateCategoryType;
using Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;
using Translator.Application.Features.CategoryTypes.Queries;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/categoryType")]
public class CategoryTypeController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<Guid> Create(CreateCategoryTypeModel model)
    {
        var command = new CreateCategoryTypeCommand(model.TypeName.ToLower().Trim());
        
        return await mediator.Send(command);
    }
    
    [HttpPost("bulk")]
    public async Task<CreateBulkCategoryTypeResponse> CreateBulk(CreateBulkCategoryTypeModel model)
    {
        var command = new CreateBulkCategoryTypeCommand(
            model.TypeNames.Select(name => name.ToLower().Trim()));
    
        return await mediator.Send(command);
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteCategoryTypeModel model)
    {
        var command = new DeleteCategoryTypeCommand(
            model.TypeNames.Select(name => name.ToLower().Trim()));
        
        await mediator.Send(command);
        
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAll()
    {
        return await mediator.Send(new GetAllTypesQuery());
    }
}