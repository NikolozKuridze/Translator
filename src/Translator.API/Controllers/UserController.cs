using System.Text;
using System.Text.Json;
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
            var normalizedUsername = username.ToLower().Trim();
        
            if (normalizedUsername == "company website")
            {
                var errorMsg = "Cannot delete the 'Company Website' user. This user is protected and cannot be deleted.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = errorMsg });
                }
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction("Index");
            }

            var command = new DeleteUser.Command(normalizedUsername);
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

    
    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] SearchUsers.Query query)
    {
        try
        {
            var result = await _mediator.Send(query);
        
            return Json(new 
            { 
                success = true, 
                data = result.Items,
                page = result.Page,
                pageSize = result.PageSize,
                totalItems = result.TotalItems,
                totalPages = (int)Math.Ceiling((double)result.TotalItems / result.PageSize),
                hasNextPage = result.HasNextPage,
                hasPreviousPage = result.HasPreviousPage
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
    
    [HttpGet("ExportJson")]
    public async Task<IActionResult> ExportJson()
    {
        try
        {
            var query = new GetUsers.Query();
            var allUsers = await _mediator.Send(query);

            var exportData = allUsers.Select(u => new
            {
                u.UserId,
                u.UserName,
                SecretKey = u.SecretKey // Keep secret key for export
            }).ToList();

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"users-export-{DateTime.UtcNow:yyyyMMdd-HHmmss}.json";
        
            Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
            Response.ContentType = "application/json";

            return File(Encoding.UTF8.GetBytes(json), "application/json", fileName);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error exporting users: {ex.Message}";
            return RedirectToAction("Index");
        }
    }

}
