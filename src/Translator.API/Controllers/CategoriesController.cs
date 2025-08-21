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
using Translator.Application.Features.CategoryTypes.Commands.CreateBulkCategoryType;

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
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
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


    [HttpPost("CreateBulkCategoryType")]
    public async Task<IActionResult> CreateBulkCategoryType(string[]? typeNames)
    {
        try
        {
            if (typeNames == null || typeNames.All(string.IsNullOrWhiteSpace))
            {
                return Json(new
                {
                    success = false,
                    error = "At least one type name is required."
                });
            }

            var command = new CreateBulkCategoryTypeCommand(
                typeNames.Where(name => !string.IsNullOrWhiteSpace(name))
                    .Select(name => name.ToLower().Trim()));

            var result = await mediator.Send(command);

            var createdCount = result.CreatedTypeNames.Count();
            var existingCount = result.ExistingTypeNames.Count();

            if (createdCount > 0 && existingCount > 0)
            {
                return Json(new
                {
                    success = true,
                    message = $"Created {createdCount} new category type(s).",
                    warning =
                        $"The following {existingCount} type(s) already exist: {string.Join(", ", result.ExistingTypeNames)}",
                    existingTypes = result.ExistingTypeNames.ToArray(),
                    createdTypes = result.CreatedTypeNames.ToArray()
                });
            }
            else if (createdCount > 0)
            {
                return Json(new
                {
                    success = true,
                    message = $"Successfully created {createdCount} category type(s).",
                    createdTypes = result.CreatedTypeNames.ToArray()
                });
            }
            else
            {
                return Json(new
                {
                    success = false,
                    error =
                        $"All category types already exist in the database: {string.Join(", ", result.ExistingTypeNames)}",
                    existingTypes = result.ExistingTypeNames.ToArray()
                });
            }
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = "An error occurred while creating category types: " + ex.Message
            });
        }
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(Guid id, string? value = null, int? order = null,
        Guid? returnToTreeId = null)
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
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = ex.Message;
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
                return Json(new
                {
                    success = false,
                    error = "Type name is required."
                });
            }

            var command = new CreateCategoryTypeCommand(typeName.ToLower().Trim());
            await mediator.Send(command);

            return Json(new
            {
                success = true,
                message = "Category type created successfully."
            });
        }
        catch (Exception ex)
        {
            // Check if it's a duplicate error
            if (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate"))
            {
                return Json(new
                {
                    success = false,
                    error = $"Category type '{typeName}' already exists in the database.",
                    existingTypes = new[] { typeName.ToLower().Trim() }
                });
            }

            return Json(new
            {
                success = false,
                error = "An error occurred while creating the category type: " + ex.Message
            });
        }
    }

    [HttpPost("DeleteCategoryTypes")]
    public async Task<IActionResult> DeleteCategoryTypes([FromBody] List<string> typeNames)
    {
        try
        {
            if (typeNames == null || typeNames.Count == 0)
            {
                return Json(new
                {
                    success = false,
                    error = "Please select at least one category type to delete."
                });
            }

            var command = new DeleteCategoryTypeCommand(typeNames);
            await mediator.Send(command);

            var typeCount = typeNames.Count;
            var message = typeCount == 1 
                ? "Category type deleted successfully."
                : $"{typeCount} category types deleted successfully.";

            return Json(new
            {
                success = true,
                message = message
            });
        }
        catch (Exception ex)
        {
            return Json(new
            {
                success = false,
                error = "An error occurred while deleting category types: " + ex.Message
            });
        }
    }
}