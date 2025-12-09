using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Translator.API.Attributes;
using Translator.Application.Features.Users.Commands;
using Translator.Application.Features.ValuesAdmin.Commands;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Landings")]
[ApiExplorerSettings(IgnoreApi = true)]
public class LandingsController : Controller
{
    private readonly IMediator _mediator;

    public LandingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("create-landing")]
    public async Task<IActionResult> CreateLanding([FromBody] CreateLandingRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { error = "Invalid request data" });
        }

        try
        {
            // Step 1: Create user first
            var createUserCommand = new AddUser.Command(request.UserName);
            var userResponse = await _mediator.Send(createUserCommand);

            // Step 2: Create values for the user using AdminCreateValue
            await CreateUserValues(request.UserName, request.Values);

            return Ok(new { 
                success = true, 
                userId = userResponse.Id,
                secretKey = userResponse.SecretKey,
                message = "Landing created successfully" 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    private async Task CreateUserValues(string userName, Dictionary<string, string> values)
    {
        foreach (var value in values)
        {
            var createValueCommand = new AdminCreateValue.Command(
                Key: value.Key,
                Value: value.Value,
                Username: userName
            );
            
            await _mediator.Send(createValueCommand);
        }
    }
}

public class CreateLandingRequest
{
    public string UserName { get; set; } = string.Empty;
    public Dictionary<string, string> Values { get; set; } = new();
}