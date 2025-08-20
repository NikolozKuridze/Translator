using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Category.Queries.GetRootCategories;
using Translator.Application.Features.Category.Commands.DeleteCategory;
using Translator.Application.Features.Category.Commands.AddCategory;
using Translator.Application.Features.Category.Commands.UpdateCategory;
using Translator.Application.Features.CategoryTypes.Queries;
using Translator.Application.Features.CategoryTypes.Commands.CreateCategoryType;
using Translator.Application.Features.CategoryTypes.Commands.DeleteCategoryType;
using Translator.Application.Features.Category.Queries.GetCategory;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Categories")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CategoriesController(IMediator mediator) : Controller
{
    [HttpGet("")] 
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await mediator.Send(new GetRootCategoriesQuery());
            var categoryTypes = await mediator.Send(new GetAllTypesQuery());
            ViewBag.CategoryTypes = categoryTypes;
            return View(categories);
        }
        catch
        {
            ViewBag.ErrorMessage = "An error occurred while loading categories.";
            ViewBag.CategoryTypes = new List<string>();
            return View(new List<RootCategoryDto>());
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string value, string type, int? order = null, Guid? parentId = null, Guid? returnToTreeId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(type))
            {
                if (returnToTreeId.HasValue)
                {
                    ViewBag.ErrorMessage = "Value and Type are required.";
                    var category = await mediator.Send(new GetCategoryQuery(returnToTreeId.Value));
                    var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                    ViewBag.CategoryTypes = categoryTypes;
                    return View("Tree", category);
                }
                else
                {
                    ViewBag.ErrorMessage = "Value and Type are required.";
                    var categories = await mediator.Send(new GetRootCategoriesQuery());
                    var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                    ViewBag.CategoryTypes = categoryTypes;
                    return View("Index", categories);
                }
            }

            var command = new CreateCategoryCommand(value.ToLower().Trim(), type.ToLower().Trim(), order, parentId);
            await mediator.Send(command);
            
            TempData["SuccessMessage"] = "Category created successfully.";
            
            // If returnToTreeId is provided, redirect back to the tree page
            if (returnToTreeId.HasValue)
            {
                return RedirectToAction("Tree", new { id = returnToTreeId.Value });
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            if (returnToTreeId.HasValue)
            {
                ViewBag.ErrorMessage = "An error occurred while creating the category: " + ex.Message;
                var category = await mediator.Send(new GetCategoryQuery(returnToTreeId.Value));
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Tree", category);
            }
            else
            {
                ViewBag.ErrorMessage = "An error occurred while creating the category: " + ex.Message;
                var categories = await mediator.Send(new GetRootCategoriesQuery());
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Index", categories);
            }
        }
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(Guid id, string? value = null, int? order = null, Guid? returnToTreeId = null)
    {
        try
        {
            var command = new UpdateCategoryCommand(id, value?.ToLower().Trim(), order);
            await mediator.Send(command);
            
            TempData["SuccessMessage"] = "Category updated successfully.";
            
            // If returnToTreeId is provided, redirect back to the tree page
            if (returnToTreeId.HasValue)
            {
                return RedirectToAction("Tree", new { id = returnToTreeId.Value });
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            if (returnToTreeId.HasValue)
            {
                ViewBag.ErrorMessage = "An error occurred while updating the category: " + ex.Message;
                var category = await mediator.Send(new GetCategoryQuery(returnToTreeId.Value));
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Tree", category);
            }
            else
            {
                ViewBag.ErrorMessage = "An error occurred while updating the category: " + ex.Message;
                var categories = await mediator.Send(new GetRootCategoriesQuery());
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Index", categories);
            }
        }
    }

    [HttpGet("Tree/{id:guid}")]
    public async Task<IActionResult> Tree(Guid id)
    {
        try
        {
            var category = await mediator.Send(new GetCategoryQuery(id));
            var categoryTypes = await mediator.Send(new GetAllTypesQuery());
            ViewBag.CategoryTypes = categoryTypes;
            
            // Pass success/error messages from TempData
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"];
            }
            
            return View(category);
        }
        catch
        {
            ViewBag.ErrorMessage = "An error occurred while loading the category tree.";
            return RedirectToAction("Index");
        }
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(Guid id, Guid? returnToTreeId = null)
    {
        try
        {
            var command = new DeleteCategoryCommand(id);
            await mediator.Send(command);
            
            TempData["SuccessMessage"] = "Category deleted successfully.";
            
            // If returnToTreeId is provided, redirect back to the tree page
            if (returnToTreeId.HasValue)
            {
                return RedirectToAction("Tree", new { id = returnToTreeId.Value });
            }
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while deleting the category: " + ex.Message;
            
            // If returnToTreeId is provided, redirect back to the tree page
            if (returnToTreeId.HasValue)
            {
                return RedirectToAction("Tree", new { id = returnToTreeId.Value });
            }
            
            return RedirectToAction("Index");
        }
    }

    [HttpPost("CreateCategoryType")]
    public async Task<IActionResult> CreateCategoryType(string typeName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                ViewBag.ErrorMessage = "Type name is required.";
                var categories = await mediator.Send(new GetRootCategoriesQuery());
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Index", categories);
            }

            var command = new CreateCategoryTypeCommand(typeName.ToLower().Trim());
            await mediator.Send(command);
            
            ViewBag.SuccessMessage = "Category type created successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "An error occurred while creating the category type: " + ex.Message;
            var categories = await mediator.Send(new GetRootCategoriesQuery());
            var categoryTypes = await mediator.Send(new GetAllTypesQuery());
            ViewBag.CategoryTypes = categoryTypes;
            return View("Index", categories);
        }
    }

    [HttpPost("DeleteCategoryType")]
    public async Task<IActionResult> DeleteCategoryType(string typeName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                ViewBag.ErrorMessage = "Type name is required.";
                var categories = await mediator.Send(new GetRootCategoriesQuery());
                var categoryTypes = await mediator.Send(new GetAllTypesQuery());
                ViewBag.CategoryTypes = categoryTypes;
                return View("Index", categories);
            }

            var command = new DeleteCategoryTypeCommand(typeName.ToLower().Trim());
            await mediator.Send(command);
            
            ViewBag.SuccessMessage = "Category type deleted successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "An error occurred while deleting the category type: " + ex.Message;
            var categories = await mediator.Send(new GetRootCategoriesQuery());
            var categoryTypes = await mediator.Send(new GetAllTypesQuery());
            ViewBag.CategoryTypes = categoryTypes;
            return View("Index", categories);
        }
    }
}
