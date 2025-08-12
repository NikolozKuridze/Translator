using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Template.Commands.CreateTemplate;
using Translator.Application.Features.Template.Commands.DeleteTemplate;
using Translator.Application.Features.Template.Queries.GetAllTemplates;
using Translator.Application.Features.Template.Queries.GetTemplate;

namespace Translator.API.Controllers
{
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
        public async Task<IActionResult> Index(
            string sortBy = "name",
            string sortDirection = "asc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var command = new GetAllTemplatesCommand(pageNumber, pageSize, sortBy, sortDirection);
            var templates = await _mediator.Send(command);

            var totalCount = templates.FirstOrDefault()?.TemplateCount ?? 0;
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDirection = sortDirection;

            return View(templates);
        }

        [HttpGet("Details")]
        public async Task<IActionResult> Details(string templateName)
        {
            if (string.IsNullOrWhiteSpace(templateName))
                return RedirectToAction(nameof(Index));

            var query = new GetTemplateCommand(templateName, null);
            var details = await _mediator.Send(query);

            ViewBag.TemplateName = templateName;
            return View(details);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(string templateName, List<string> values)
        {
            var command = new CreateTemplateCommand(templateName.Trim(), values);
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("Delete")]
        public async Task<IActionResult> Delete(string templateName)
        {
            var command = new DeleteTemplateCommand(templateName.Trim());
            await _mediator.Send(command);
            return RedirectToAction(nameof(Index));
        }
    }
}
