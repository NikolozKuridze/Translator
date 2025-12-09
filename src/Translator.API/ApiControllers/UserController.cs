using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Features.Users.Commands;

namespace Translator.API.ApiControllers;

[ApiController]
[Route("api/users")]
public class UserController(ISender mediator) : ControllerBase
{
    [HttpDelete]
    public async Task<IResult> Delete([FromQuery] string username)
    {
        var command = new DeleteUser.Command(username);
        await mediator.Send(command);
        return Results.NoContent();
    }
}