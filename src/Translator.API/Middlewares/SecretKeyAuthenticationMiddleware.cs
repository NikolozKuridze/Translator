using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Translator.Domain.Entities;
using Translator.Infrastructure.Database.Postgres.Repository;

namespace Translator.API.Middlewares;

public class SecretKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SecretKeyAuthenticationMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower();
        if (path == null ||
            path.Contains("/auth/") ||
            path.Contains("/admin") ||
            path.Contains("/users") ||
            path.Contains("/categories") ||
            path.Contains("/languages") ||
            path.Contains("/templates") ||
            path.Contains("/values") ||
            path.Contains("/logs") ||
            path.Contains("/openapi") ||
            path.Contains("/docs") ||
            path.StartsWith("/home") ||
            !path.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-Secret-Key", out var secretKeyValues) ||
            string.IsNullOrWhiteSpace(secretKeyValues.FirstOrDefault()))
        {
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                "{\"success\": false, \"message\": \"Secret key is required in X-Secret-Key header\"}");
            return;
        }

        var secretKey = secretKeyValues.FirstOrDefault()!.Trim();

        using var scope = _serviceScopeFactory.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IRepository<User>>();

        try
        {
            var user = await userRepository.AsQueryable()
                .FirstOrDefaultAsync(u => u.SecretKey == secretKey);

            if (user == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\": false, \"message\": \"Invalid secret key\"}");
                return;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("SecretKey", user.SecretKey)
            };

            var identity = new ClaimsIdentity(claims, "SecretKey");
            context.User = new ClaimsPrincipal(identity);

            await _next(context);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                $"{{\"success\": false, \"message\": \"Authentication error: {ex.Message}\"}}");
        }
    }
}