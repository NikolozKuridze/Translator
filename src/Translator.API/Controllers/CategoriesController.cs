using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Exceptions;
using Translator.Application.Features.Category.Commands;
using Translator.Application.Features.Category.Queries;
using Translator.Application.Features.CategoryTypes.Commands;
using Translator.Application.Features.CategoryTypes.Queries;
using Translator.Domain.Pagination;

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Categories")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CategoriesController(IMediator mediator) : Controller
{
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? search = null, int page = 1, int pageSize = 10)
    {
        try
        {
            var paginationRequest = new PaginationRequest(page, pageSize, null, null, null, null);
            
            PaginatedResponse<GetRootCategories.Response> categories;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchQuery = new SearchRootCategories.Query(search, paginationRequest);
                categories = await mediator.Send(searchQuery);
            }
            else
            {
                var getRootQuery = new GetRootCategories.Query(paginationRequest);
                categories = await mediator.Send(getRootQuery);
            }
            
            var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    categories = categories.Items.ToList(),
                    categoryTypes = categoryTypes.TypeNames.ToList(),
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                });

            // Ensure ViewBag has proper data
            ViewBag.CategoryTypes = categoryTypes.TypeNames.ToList();
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            
            return View(categories);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    categories = new List<GetRootCategories.Response>(),
                    categoryTypes = new List<string>(),
                    pagination = new
                    {
                        page = 1,
                        pageSize = 10,
                        totalItems = 0,
                        totalPages = 0,
                        hasNextPage = false,
                        hasPreviousPage = false,
                        isFirstPage = true,
                        isLastPage = true
                    }
                });

            ViewBag.ErrorMessage = ex.Message;
            ViewBag.CategoryTypes = new List<string>();
            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            return View(new PaginatedResponse<GetRootCategories.Response>());
        }
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(
        string value,
        string type,
        string? metadata,
        string? shortcode,
        int? order,
        Guid? parentId,
        Guid? returnToTreeId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(type))
            {
                var errorMessage = "Value and Type are required.";
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = errorMessage });

                TempData["ErrorMessage"] = errorMessage;
                return returnToTreeId.HasValue
                    ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                    : RedirectToAction("Index");
            }

            var command = new AddCategory.Command(value, type, metadata, shortcode, order, parentId);
            var result = await mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (returnToTreeId.HasValue)
                {
                    var treeCategory = await mediator.Send(new GetCategoryTree.Query(returnToTreeId.Value));
                    var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                    return Json(new
                    {
                        success = true,
                        message = "Category created successfully.",
                        categoryId = result,
                        treeData = treeCategory,
                        categoryTypes = categoryTypes.TypeNames.ToList()
                    });
                }

                // Return updated data for index page
                var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
                var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));
                var updatedCategoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                return Json(new
                {
                    success = true,
                    message = "Category created successfully.",
                    categoryId = result,
                    categories = categories.Items.ToList(),
                    categoryTypes = updatedCategoryTypes.TypeNames.ToList(),
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                });
            }

            TempData["SuccessMessage"] = "Category created successfully.";
            return returnToTreeId.HasValue
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while creating the category: " + ex.Message;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMessage });

            TempData["ErrorMessage"] = errorMessage;
            return returnToTreeId.HasValue
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
    }

    [HttpPost("Update")]
    public async Task<IActionResult> Update(Guid id, string? value, string? metadata, string? shortcode, int? order,
        Guid? returnToTreeId = null)
    {
        try
        {
            var command = new UpdateCategory.Command(id, value, metadata, shortcode, order);
            await mediator.Send(command);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (returnToTreeId.HasValue)
                {
                    var treeCategory = await mediator.Send(new GetCategoryTree.Query(returnToTreeId.Value));
                    var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                    return Json(new
                    {
                        success = true,
                        message = "Category updated successfully.",
                        treeData = treeCategory,
                        categoryTypes = categoryTypes.TypeNames.ToList()
                    });
                }

                // Return updated data for index page
                var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
                var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));
                var updatedCategoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                return Json(new
                {
                    success = true,
                    message = "Category updated successfully.",
                    categories = categories.Items.ToList(),
                    categoryTypes = updatedCategoryTypes.TypeNames.ToList(),
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                });
            }

            TempData["SuccessMessage"] = "Category updated successfully.";
            return returnToTreeId.HasValue
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while updating the category: " + ex.Message;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMessage });

            TempData["ErrorMessage"] = errorMessage;
            return returnToTreeId.HasValue
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(Guid id, Guid? returnToTreeId = null)
    {
        try
        {
            var command = new DeleteCategory.Command(id);
            await mediator.Send(command);

            // If we're deleting the root category itself (the one we're viewing the tree of),
            // redirect to index instead of trying to return to the deleted category's tree
            bool isRootCategoryDeleted = returnToTreeId.HasValue && returnToTreeId.Value == id;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (returnToTreeId.HasValue && !isRootCategoryDeleted)
                {
                    var treeCategory = await mediator.Send(new GetCategoryTree.Query(returnToTreeId.Value));
                    var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                    return Json(new
                    {
                        success = true,
                        message = "Category deleted successfully.",
                        treeData = treeCategory,
                        categoryTypes = categoryTypes.TypeNames.ToList()
                    });
                }

                // Return updated data for index page
                var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
                var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));
                var updatedCategoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
                return Json(new
                {
                    success = true,
                    message = "Category deleted successfully.",
                    categories = categories.Items.ToList(),
                    categoryTypes = updatedCategoryTypes.TypeNames.ToList(),
                    redirectToIndex = isRootCategoryDeleted, // Flag to indicate redirect needed
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                });
            }

            TempData["SuccessMessage"] = "Category deleted successfully.";
            return (returnToTreeId.HasValue && !isRootCategoryDeleted)
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            var errorMessage = "An error occurred while deleting the category: " + ex.Message;
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = errorMessage });

            TempData["ErrorMessage"] = errorMessage;
            
            // On error, also avoid returning to deleted category tree
            bool isRootCategoryDeleted = returnToTreeId.HasValue && returnToTreeId.Value == id;
            return (returnToTreeId.HasValue && !isRootCategoryDeleted)
                ? RedirectToAction("Tree", new { id = returnToTreeId.Value })
                : RedirectToAction("Index");
        }
    }

    [HttpPost("CreateCategoryType")]
    public async Task<IActionResult> CreateCategoryType(string typeName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return Json(new { success = false, error = "Type name is required." });

            // The command handler will convert to lowercase
            var command = new AddCategoryType.Command(typeName);
            await mediator.Send(command);

            var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
            var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
            var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));

            return Json(new
            {
                success = true,
                message = "Category type created successfully.",
                categoryTypes = categoryTypes.TypeNames.ToList(),
                categories = categories.Items.ToList(),
                pagination = new
                {
                    page = categories.Page,
                    pageSize = categories.PageSize,
                    totalItems = categories.TotalItems,
                    totalPages = categories.TotalPages,
                    hasNextPage = categories.HasNextPage,
                    hasPreviousPage = categories.HasPreviousPage,
                    isFirstPage = categories.IsFirstPage,
                    isLastPage = categories.IsLastPage
                }
            });
        }
        catch (ValidationException ex)
        {
            return Json(new
            {
                success = false,
                error = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                validationErrors = ex.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
                    .ToArray()
            });
        }
        catch (TypeAlreadyExistsException ex)
        {
            return Json(
                new { success = false, error = ex.Message, existingTypes = new[] { typeName.Trim().ToLower() } });
        }
        catch (Exception ex)
        {
            return Json(new
                { success = false, error = "An error occurred while creating the category type: " + ex.Message });
        }
    }

    [HttpPost("CreateBulkCategoryType")]
    public async Task<IActionResult> CreateBulkCategoryType(string[]? typeNames)
    {
        try
        {
            if (typeNames == null || typeNames.All(string.IsNullOrWhiteSpace))
                return Json(new { success = false, error = "At least one type name is required." });

            var cleanedTypeNames = typeNames.Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n.Trim()).ToArray();

            // The command handler will convert to lowercase
            var command = new AddBulkCategoryTypes.Command(cleanedTypeNames);
            var result = await mediator.Send(command);

            var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
            var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
            var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));

            var createdCount = result.CreatedTypeNames.Count();
            var existingCount = result.ExistingTypeNames.Count();

            return createdCount switch
            {
                > 0 when existingCount > 0 => Json(new
                {
                    success = true,
                    message = $"Created {createdCount} new category type(s).",
                    warning =
                        $"The following {existingCount} type(s) already exist: {string.Join(", ", result.ExistingTypeNames)}",
                    existingTypes = result.ExistingTypeNames.ToArray(),
                    createdTypes = result.CreatedTypeNames.ToArray(),
                    categoryTypes = categoryTypes.TypeNames.ToList(),
                    categories = categories.Items.ToList(),
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                }),
                > 0 => Json(new
                {
                    success = true,
                    message = $"Successfully created {createdCount} category type(s).",
                    createdTypes = result.CreatedTypeNames.ToArray(),
                    categoryTypes = categoryTypes.TypeNames.ToList(),
                    categories = categories.Items.ToList(),
                    pagination = new
                    {
                        page = categories.Page,
                        pageSize = categories.PageSize,
                        totalItems = categories.TotalItems,
                        totalPages = categories.TotalPages,
                        hasNextPage = categories.HasNextPage,
                        hasPreviousPage = categories.HasPreviousPage,
                        isFirstPage = categories.IsFirstPage,
                        isLastPage = categories.IsLastPage
                    }
                }),
                _ => Json(new
                {
                    success = false,
                    error = $"All category types already exist: {string.Join(", ", result.ExistingTypeNames)}",
                    existingTypes = result.ExistingTypeNames.ToArray()
                })
            };
        }
        catch (ValidationException ex)
        {
            return Json(new
            {
                success = false,
                error = "Validation failed: " + string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                validationErrors = ex.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
                    .ToArray()
            });
        }
        catch (Exception ex)
        {
            return Json(new
                { success = false, error = "An error occurred while creating category types: " + ex.Message });
        }
    }

    [HttpPost("DeleteCategoryTypes")]
    public async Task<IActionResult> DeleteCategoryTypes([FromBody] List<string>? typeNames)
    {
        try
        {
            if (typeNames == null || typeNames.Count == 0)
                return Json(new { success = false, error = "Please select at least one category type to delete." });

            var cleanedTypeNames = typeNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            var command = new DeleteCategoryTypes.Command(cleanedTypeNames);
            await mediator.Send(command);

            var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());
            var paginationRequest = new PaginationRequest(1, 10, null, null, null, null);
            var categories = await mediator.Send(new GetRootCategories.Query(paginationRequest));

            var message = cleanedTypeNames.Count == 1
                ? "Category type deleted successfully."
                : $"{cleanedTypeNames.Count} category types deleted successfully.";

            return Json(new
            {
                success = true,
                message,
                categoryTypes = categoryTypes.TypeNames.ToList(),
                categories = categories.Items.ToList(),
                pagination = new
                {
                    page = categories.Page,
                    pageSize = categories.PageSize,
                    totalItems = categories.TotalItems,
                    totalPages = categories.TotalPages,
                    hasNextPage = categories.HasNextPage,
                    hasPreviousPage = categories.HasPreviousPage,
                    isFirstPage = categories.IsFirstPage,
                    isLastPage = categories.IsLastPage
                }
            });
        }
        catch (ValidationException ex)
        {
            return Json(new
            {
                success = false,
                error = "Validation failed: " + string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)),
                validationErrors = ex.Errors.Select(e => new { property = e.PropertyName, error = e.ErrorMessage })
                    .ToArray()
            });
        }
        catch (TypeNotFoundException ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
        catch (Exception ex)
        {
            return Json(new
                { success = false, error = "An error occurred while deleting category types: " + ex.Message });
        }
    }

    [HttpGet("Tree/{id:guid}")]
    public async Task<IActionResult> Tree(Guid id)
    {
        try
        {
            var category = await mediator.Send(new GetCategoryTree.Query(id));
            var categoryTypes = await mediator.Send(new GetAllCategoryTypes.Query());

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new
                {
                    success = true,
                    treeData = category,
                    categoryTypes = categoryTypes.TypeNames.ToList()
                });

            ViewBag.CategoryTypes = categoryTypes.TypeNames.ToList();

            if (TempData["SuccessMessage"] != null)
                ViewBag.SuccessMessage = TempData["SuccessMessage"];

            if (TempData["ErrorMessage"] != null)
                ViewBag.ErrorMessage = TempData["ErrorMessage"];

            return View(category);
        }
        catch (Exception ex)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = ex.Message });

            ViewBag.ErrorMessage = ex.Message;
            return RedirectToAction("Index");
        }
    }
}
