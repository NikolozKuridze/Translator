using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Translator.Application.Exceptions;

namespace Translator.API.Middlewares;

public class MvcExceptionFilter : IExceptionFilter
{
    private readonly ILogger<MvcExceptionFilter> _logger;

    public MvcExceptionFilter(ILogger<MvcExceptionFilter> logger)
    {
        _logger = logger;
    }
    public void OnException(ExceptionContext context)
    {
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
            return;
     
        _logger.LogError(context.Exception, context.Exception.Message);
        
        var model = new ErrorViewModel
        {
            Title = "Something went wrong",
            Message = "We already working for problem",
            RequestId = context.HttpContext.TraceIdentifier
        };

        if (context.Exception is ApplicationLayerException ex)
        {
            model.Title = "Error";
            model.Message = ex.Message;
        }

        var result = new ViewResult
        {
            ViewName = "Error", // Views/Shared/Error.cshtml
            ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ErrorViewModel>(
                new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(), context.ModelState)
            {
                Model = model
            }
        };

        context.Result = result;
        context.ExceptionHandled = true;
        context.HttpContext.Response.StatusCode = model.Title == "Error" ? 400 : 500;
    }
}

public class ErrorViewModel
{
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? RequestId { get; set; }
}
