

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Translator.Application.Exceptions;

namespace Translator.API.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ILogger<ErrorHandlingMiddleware> logger,
        ProblemDetailsFactory problemDetailsFactory)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Error has happened with {RequestPath}, the message is {ErrorMessage}",
                context.Request.Path, exception.Message);

            ProblemDetails problemDetails;

            switch (exception)
            {
                case ValidationException validationException:
                    problemDetails = problemDetailsFactory.CreateFrom(context, validationException);
                    logger.LogError(exception, "It's validation error");
                    break;
                case ApplicationLayerException applicationException:
                    problemDetails = problemDetailsFactory.CreateFrom(context, applicationException);
                    logger.LogError(exception, "Application exception occured");
                    break;
                default:
                    problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context,
                        StatusCodes.Status500InternalServerError,
                        "Oops! it's unhandled error, please, contact us!", exception.Message);
                    logger.LogError(exception, "Oops! it's unhandled error");
                    break;
            }
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType());
        }
    }
}