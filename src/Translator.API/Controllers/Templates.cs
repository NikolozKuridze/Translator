using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Template.Queries.GetTemplate;

namespace Translator.API.Controllers;

public class Templates : Controller
{
    private readonly IMediator _mediator;

    public Templates(IMediator mediator)
    {
        _mediator = mediator;
    }
       
    public async Task<IActionResult> Index(string templateName, string? lang)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            return View(Enumerable.Empty<TemplateDto>()); // Пустое отображение

        try
        {
            var query = new GetTemplateCommand(templateName, lang);
            var templates = await _mediator.Send(query); // templates: IEnumerable<TemplateDto>
            return View(templates);
        }
        catch (ApplicationLayerException ex)
        {
            ViewBag.Error = $"Ошибка: {ex.Message}, {ex.ErrorCode}";
            return View(Enumerable.Empty<TemplateDto>());
        }
    }
}