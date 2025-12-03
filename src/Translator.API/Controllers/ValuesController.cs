using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.Translation.Commands;
using Translator.Application.Features.Users.Queries;
using Translator.Application.Features.ValuesAdmin.Commands;
using Translator.Application.Features.ValuesAdmin.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("Values")]
public class ValuesController : Controller
{
    private readonly IMediator _mediator;

    public ValuesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        string sortBy = "date",
        string sortDirection = "desc",
        int pageNumber = 1,
        int pageSize = 10,
        string userName = "")
    {
        try
        {
            var paginationRequest =
                new PaginationRequest(pageNumber, pageSize, null, null, null, sortBy, sortDirection);

            var command = new AdminSearchValue.Command(null, userName, paginationRequest);
            var result = await _mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    data = result.Items,
                    currentPage = result.Page,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalItems,
                    pageSize = result.PageSize,
                    sortBy,
                    sortDirection,
                    searchKey = "",
                    searchUserName = userName
                });

            ViewBag.CurrentPage = result.Page;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalItems;
            ViewBag.HasNextPage = result.HasNextPage;
            ViewBag.HasPreviousPage = result.HasPreviousPage;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;
            ViewBag.UserName = userName;

            ViewBag.GlobalCount = result.Items.Count(x => x.OwnershipType == "Global");
            ViewBag.UserCount = result.Items.Count(x => x.OwnershipType == "User");

            return View(result.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = $"Error loading values: {ex.Message}";
            return View(new List<AdminGetAllValues.Response>());
        }
    }

    [HttpGet("Details/{valueId:guid}")]
    public async Task<IActionResult> Details(Guid valueId, string? lang)
    {
        try
        {
            if (valueId == Guid.Empty)
                return RedirectToAction(nameof(Index));

            var command = new AdminGetValue.Command(valueId, lang?.Trim(), true);
            var result = await _mediator.Send(command);

            var languagesQuery = new GetLanguagesCommand();
            var availableLanguages = (await _mediator.Send(languagesQuery))
                .Where(l => l.IsActive)
                .OrderBy(l => l.LanguageCode)
                .ToList();

            var firstResult = result.First();
            ViewBag.ValueId = valueId;
            ViewBag.ValueKey = firstResult.ValueKey;
            ViewBag.OwnerId = firstResult.OwnerId;
            ViewBag.OwnerName = firstResult.OwnerName;
            ViewBag.OwnershipType = firstResult.OwnershipType;
            ViewBag.CurrentLanguage = lang ?? "";
            ViewBag.AvailableLanguages = availableLanguages;

            return View(result);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error loading value details: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet("Search")]
    public async Task<IActionResult> Search(
        string valueKey = "",
        string userName = "",
        string sortBy = "date",
        string sortDirection = "desc",
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            var paginationRequest =
                new PaginationRequest(pageNumber, pageSize, null, null, null, sortBy, sortDirection);

            // Update to include userName
            var command = new AdminSearchValue.Command(valueKey, userName, paginationRequest);
            var result = await _mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    data = result.Items,
                    currentPage = result.Page,
                    totalPages = result.TotalPages,
                    totalCount = result.TotalItems,
                    pageSize = result.PageSize,
                    sortBy,
                    sortDirection,
                    searchKey = valueKey,
                    searchUserName = userName // Add this
                });

            ViewBag.CurrentPage = result.Page;
            ViewBag.PageSize = result.PageSize;
            ViewBag.TotalPages = result.TotalPages;
            ViewBag.TotalCount = result.TotalItems;
            ViewBag.HasNextPage = result.HasNextPage;
            ViewBag.HasPreviousPage = result.HasPreviousPage;
            ViewBag.ValueKey = valueKey;
            ViewBag.UserName = userName; // Add this
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            ViewBag.GlobalCount = result.Items.Count(x => x.OwnershipType == "Global");
            ViewBag.UserCount = result.Items.Count(x => x.OwnershipType == "User");

            return View("Index", result.Items);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            TempData["ErrorMessage"] = $"Error searching values: {ex.Message}";
            return View("Index", new List<AdminGetAllValues.Response>());
        }
    }

    [HttpGet("GetUsers")]
    public async Task<IActionResult> GetUsers(string search = "")
    {
        try
        {
            var query = new GetUsers.Query();
            var users = await _mediator.Send(query);
            
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            
            return Json(new { success = true, users = users.ToList() });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string key, string value, string? username = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                var errorMsg = "Key and value are required.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var command = new AdminCreateValue.Command(key.Trim(), value.Trim(), username?.Trim());
            await _mediator.Send(command);
            
            var successMsg = string.IsNullOrEmpty(username) 
                ? "Global value created successfully!" 
                : $"Value created successfully for user '{username}'!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error creating value: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string valueName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(valueName))
            {
                var errorMsg = "Value name is required for deletion.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var allValues =
                await _mediator.Send(
                    new AdminGetAllValues.Command(new PaginationRequest(1, 1000, null, null, null, null, null)));
            var valueToDelete =
                allValues.Items.FirstOrDefault(v => v.Key.Equals(valueName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (valueToDelete == null)
            {
                var errorMsg = $"Value '{valueName}' not found.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Index));
            }

            var command = new AdminDeleteValue.Command(valueToDelete.ValueId);
            await _mediator.Send(command);
            var successMsg = "Value deleted successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting value: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{valueId:guid}")]
    public async Task<IActionResult> DeleteById(Guid valueId)
    {
        try
        {
            var command = new AdminDeleteValue.Command(valueId);
            await _mediator.Send(command);
            var successMsg = "Value deleted successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = successMsg });

            TempData["SuccessMessage"] = successMsg;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting value: {ex.Message}";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("CreateTranslation")]
    public async Task<IActionResult> CreateTranslation(string value, Guid ValueId, string translation,
        string languageCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(translation) ||
                string.IsNullOrWhiteSpace(languageCode))
            {
                var errorMsg = "All fields are required for creating translation.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction("Details", new { valueId = ValueId });
            }

            var command = new CreateTranslation.Command(value.Trim(), translation.Trim(), languageCode.Trim());
            await _mediator.Send(command);

            var successMsg = "Translation created successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var updatedResult = await _mediator.Send(new AdminGetValue.Command(ValueId, null, true));
                return Json(new
                {
                    success = true,
                    message = successMsg,
                    translations = updatedResult.ToList()
                });
            }

            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction("Details", new { valueId = ValueId });
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error creating translation: {ex.Message}";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Details", new { valueId = ValueId });
        }
    }

    [HttpPost("DeleteTranslation")]
    public async Task<IActionResult> DeleteTranslation(string value, Guid ValueId, string languageCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(languageCode))
            {
                var errorMsg = "Value and language code are required for deleting translation.";

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMsg });

                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction("Details", new { valueId = ValueId });
            }

            var command = new DeleteTranslation.Command(value.Trim(), languageCode.Trim());
            await _mediator.Send(command);

            var successMsg = "Translation deleted successfully!";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var updatedResult = await _mediator.Send(new AdminGetValue.Command(ValueId, null, true));
                return Json(new
                {
                    success = true,
                    message = successMsg,
                    translations = updatedResult.ToList()
                });
            }

            TempData["SuccessMessage"] = successMsg;
            return RedirectToAction("Details", new { valueId = ValueId });
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error deleting translation: {ex.Message}";

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });

            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Details", new { valueId = ValueId });
        }
    }

[HttpPost("AdminUpdateTranslation")]
public async Task<IActionResult> AdminUpdateTranslation(string valueKey, string languageCode,
    string translationValue, Guid valueId, Guid? ownerId = null) // Add ownerId parameter
{
    try
    {
        if (string.IsNullOrWhiteSpace(valueKey) || string.IsNullOrWhiteSpace(languageCode) ||
            string.IsNullOrWhiteSpace(translationValue))
        {
            var errorMsg = "Value key, language code, and translation value are required.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMsg });
            TempData["ErrorMessage"] = errorMsg;
            return RedirectToAction("Details", new { valueId = valueId });
        }

        // Pass ownerId to the command
        var command =
            new AdminUpdateValueTranslation.Command(
                valueKey.Trim(), 
                languageCode.Trim(), 
                translationValue.Trim(),
                ownerId); // Pass ownerId
        var result = await _mediator.Send(command);

        var successMsg = $"Translation for {languageCode.ToUpper()} updated successfully!";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            var updatedResult = await _mediator.Send(new AdminGetValue.Command(valueId, null, true));
            return Json(new
            {
                success = true,
                message = successMsg,
                translations = updatedResult.ToList(),
                key = result.Key
            });
        }

        TempData["SuccessMessage"] = successMsg;
    }
    catch (Exception ex)
    {
        var errorMsg = $"Error updating translation: {ex.Message}";
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { success = false, message = errorMsg });
        TempData["ErrorMessage"] = errorMsg;
    }

    return RedirectToAction("Details", new { valueId = valueId });
}
}