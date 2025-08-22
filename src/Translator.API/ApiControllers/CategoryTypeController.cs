using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Models;
using Translator.Application.Features.CategoryTypes.Commands;
using Translator.Application.Features.CategoryTypes.Queries;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/categoryType")]
public class CategoryTypeController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IResult> Create(
        [FromBody] CreateCategoryTypeModel request)
    {
        var command = new AddCategoryType.Command(request.TypeName);
        
        var result = await mediator.Send(command);
        
        return Results.Ok(result);
    }
    
    [HttpPost("bulk")]
    public async Task<IResult> CreateBulk(
        [FromBody] CreateBulkCategoryTypeModel request)
    {
        var command = new AddBulkCategoryTypes.Command(request.TypeNames);
        
        var result = await mediator.Send(command);
        
        return Results.Ok(result);
    }

    [HttpDelete]
    public async Task<IResult> Delete(DeleteCategoryTypeModel request)
    {
        var command = new DeleteCategoryTypes.Command(request.TypeNames);
        
        await mediator.Send(command);
        
        return Results.Ok();
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        var query = new GetAllCategoryTypes.Query();
        
        var result = await mediator.Send(query);
        
        return Results.Ok(result);
    }
}