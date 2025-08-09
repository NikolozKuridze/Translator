using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Application.Features.Template.Queries.GetTemplate;

namespace Translator.API.Controllers;

[Route("Templates")]

[ApiExplorerSettings(IgnoreApi = true)]
public class Templates : Controller
{
    private readonly IMediator _mediator;

    public Templates(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("")]
    public async Task<IActionResult> Index(string templateName, string? lang)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            return View(Enumerable.Empty<TemplateDto>()); 

        var query = new GetTemplateCommand(templateName, lang);
        var templates = await _mediator.Send(query);
        return View(templates);
    }
    
    [HttpGet("All")]
    public async Task<IActionResult> GetAllTemplates(int pageNumber = 1, int pageSize = 10)
    {
        var command = new GetAllTemplatesCommand(pageNumber, pageSize);
        var templates = await _mediator.Send(command);
        
        ViewBag.CurrentPage = pageNumber;
        ViewBag.PageSize = pageSize;
        
        return View("AllTemplates", templates);
    }

    
    [HttpPost("Create")]
    public async Task<IActionResult> Create(string templateName, List<string> values)
    {
        var command = new CreateTemplateCommand(templateName.Trim(), values);
        await _mediator.Send(command);
        
        TempData["SuccessMessage"] = "Template created successfully!";
        return RedirectToAction("Index");
    }
    
    [HttpPost("Delete")]
    public async Task<IActionResult> Delete(string templateName)
    {
        var command = new DeleteTemplateCommand(templateName.Trim());
        await _mediator.Send(command);

        TempData["SuccessMessage"] = "Template deleted successfully!";
        return RedirectToAction("Index");
    }
}