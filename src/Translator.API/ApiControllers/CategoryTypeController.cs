using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Contracts;
using Translator.Application.Features.CategoryTypes.Commands.CreateCategoryType;
using Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;
using Translator.Application.Features.CategoryTypes.Queries;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/categoryType")]
public class CategoryTypeController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<Guid> Create(CreateCategoryTypeContract contract)
    {
        var command = new CreateCategoryTypeCommand(contract.TypeName.ToLower().Trim());
        
        return await mediator.Send(command);
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(DeleteCategoryTypeContract contract)
    {
        var command = new DeleteCategoryTypeCommand(contract.TypeName.ToLower().Trim());
        
        await mediator.Send(command);
        
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<List<string>>> GetAll()
    {
        return await mediator.Send(new GetAllTypesQuery());
    }
}