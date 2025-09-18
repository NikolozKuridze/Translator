using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Translator.API.Attributes;

public class UserAuthAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = "Authentication required",
                message = "Valid X-Secret-Key header is required"
            });
            return;
        }

        base.OnActionExecuting(context);
    }
}