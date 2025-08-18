using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.API.Attributes;
using Translator.Application.Features.Category.Queries.GetRootCategories;
using Translator.Application.Features.Category.Commands.DeleteCategory;
using Translator.Application.Features.Category.Commands.AddCategory; // Add this

namespace Translator.API.Controllers;

[AdminAuth]
[Route("Categories")]
[ApiExplorerSettings(IgnoreApi = true)]
public class CategoriesController : Controller
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")] 
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var categories = await _mediator.Send(new GetRootCategoriesQuery());
            return View(categories);
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "An error occurred while loading categories.";
            return View(new List<RootCategoryDto>());
        }
    }

    // Add this POST action
    [HttpPost("Create")]
    public async Task<IActionResult> Create(string value, string type, int? order = null, Guid? parentId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(type))
            {
                ViewBag.ErrorMessage = "Value and Type are required.";
                return View();
            }

            var command = new CreateCategoryCommand(value.ToLower().Trim(), type.ToLower().Trim(), order, parentId);
            await _mediator.Send(command);
            
            ViewBag.SuccessMessage = "Category created successfully.";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "An error occurred while creating the category.";
            return View();
        }
    }

    [HttpGet("Edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        return View();
    }

    [HttpGet("Tree/{id:guid}")]
    public async Task<IActionResult> Tree(Guid id)
    {
        return View();
    }

    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var command = new DeleteCategoryCommand(id);
            await _mediator.Send(command);
            
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ViewBag.ErrorMessage = "An error occurred while deleting the category.";
            
            var categories = await _mediator.Send(new GetRootCategoriesQuery());
            return View("Index", categories);
        }
    }
}
