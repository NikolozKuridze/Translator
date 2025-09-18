using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Users.Commands;
using Translator.Application.Features.Users.Queries;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Users")]
[ApiExplorerSettings(IgnoreApi = true)]
public class UserController : Controller
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var query = new GetUsers.Query();
            var allUsers = (await _mediator.Send(query)).ToList();

            var orderedUsers = allUsers
                .OrderBy(u => u.UserName)
                .ToList();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    data = orderedUsers
                });
            }

            return View(orderedUsers);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = ex.Message });
            }

            TempData["ErrorMessage"] = $"Error loading users: {ex.Message}";
            return View(new List<GetUsers.Response>());
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] AddUser.Command request)
    {
        try
        {
            var result = await _mediator.Send(request);

            var successMsg = $"User '{request.UserName}' created successfully! Secret Key: {result.SecretKey}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMsg, secretKey = result.SecretKey });
            }

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error creating user: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("Index");
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string username)
    {
        try
        {
            var command = new DeleteUser.Command(username.ToLower().Trim());
            await _mediator.Send(command);

            var successMsg = $"User '{username}' deleted successfully!";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true, message = successMsg });
            }

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting user: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = false, message = errorMsg });
            }

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction("Index");
    }
}
