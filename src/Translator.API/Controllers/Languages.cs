using MediatR;
using Microsoft.AspNetCore.Mvc;
using Translator.Application.Exceptions;
using Translator.Application.Features.Language.Queries.GetLanguages;
using Translator.Application.Features.Language.Commands.AddLanguage;
using Translator.Application.Features.Language.Commands.DeleteLanguage;

namespace Translator.API.Controllers
{
    [Route("Languages")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class Languages : Controller
    {
        private readonly IMediator _mediator;

        public Languages(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            string filterName = null,
            bool? filterActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {

            var query = new GetLanguagesCommand();
            var allLanguages = (await _mediator.Send(query)).ToList();

            var filteredLanguages = allLanguages.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filterName))
            {
                filteredLanguages = filteredLanguages
                    .Where(l => l.LanguageCode.Contains(filterName, StringComparison.OrdinalIgnoreCase) ||
                               (!string.IsNullOrEmpty(l.LanguageName) && l.LanguageName.Contains(filterName, StringComparison.OrdinalIgnoreCase)));
            }

            if (filterActive.HasValue)
            {
                filteredLanguages = filteredLanguages
                    .Where(l => l.IsActive == filterActive.Value);
            }

            filteredLanguages = filteredLanguages
                .OrderByDescending(l => l.IsActive)
                .ThenBy(l => l.LanguageCode);

            var totalCount = filteredLanguages.Count();
            var totalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 1;
            pageNumber = Math.Max(1, Math.Min(pageNumber, totalPages));

            var pagedLanguages = filteredLanguages
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.FilterName = filterName ?? "";
            ViewBag.FilterActive = filterActive;

            return View(pagedLanguages);

            
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create(string code)
        {
            var command = new AddLanguageCommand(code.ToLower().Trim());
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Language added successfully!";
    
            return RedirectToAction("Index");
        }

        [HttpPost("Deactivate")]
        public async Task<IActionResult> Deactivate(string code)
        {
            var command = new DeleteLanguageCommand(code);
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Language deactivated successfully!";
            return RedirectToAction("Index");
        }
        
        [HttpPost("Activate")]
        public async Task<IActionResult> Activate(string code)
        {
            var command = new AddLanguageCommand(code.ToLower().Trim());
            await _mediator.Send(command);
            
            TempData["SuccessMessage"] = "Language activated successfully!";

            return RedirectToAction("Index");
        }
    }
}
