using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Translator.API.Models;

namespace Translator.API.Attributes;

public class AdminAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        try
        {
            var response = context.HttpContext.Response;
            response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate, private");
            response.Headers.Append("Pragma", "no-cache");
            response.Headers.Append("Expires", "0");
            response.Headers.Append("Last-Modified", DateTime.UtcNow.ToString("R"));

            var authSettings = context.HttpContext.RequestServices
                .GetRequiredService<IOptions<AdminAuthSettings>>().Value;

            var sessionValue = context.HttpContext.Session
                .GetString(authSettings.SessionName);

            var isAuthenticated = !string.IsNullOrEmpty(sessionValue) && 
                                  sessionValue == "authenticated";

            if (!isAuthenticated)
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<AdminAuthAttribute>>();
                
                logger.LogWarning("Unauthorized access attempt to {Path} from {IP}", 
                    context.HttpContext.Request.Path,
                    context.HttpContext.Connection.RemoteIpAddress);

                var returnUrl = context.HttpContext.Request.Path + 
                                context.HttpContext.Request.QueryString;
                
                context.Result = new RedirectToActionResult("Login", "Auth", 
                    new { returnUrl });
                return;
            }
        }
        catch (Exception ex)
        {
            var logger = context.HttpContext.RequestServices
                .GetRequiredService<ILogger<AdminAuthAttribute>>();
            
            logger.LogError(ex, "Session validation error");
            
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        base.OnActionExecuting(context);
    }
}
