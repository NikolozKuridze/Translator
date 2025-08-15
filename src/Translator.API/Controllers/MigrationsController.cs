using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Migrations.Commands;

namespace Translator.API.Controllers;

[AdminAuth]
[ApiExplorerSettings(IgnoreApi = true)]
public class MigrationsController : Controller
{
    private readonly IMediator _mediator;

    public MigrationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IResult> ApplyMigrations()
    {
        try
        {
            var command = new ApplyMigrationsCommand();
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Migrations applied successfully!";
            return Results.Ok(new { success = true, message = "Migrations applied successfully!" });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to apply migrations: {ex.Message}";
            return Results.BadRequest(new { success = false, message = ex.Message });
        }
    }
}
