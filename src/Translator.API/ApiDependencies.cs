using Scalar.AspNetCore;
using Serilog;
using Translator.API.Attributes;
using Translator.API.Middlewares;
using Translator.API.Models;

namespace Translator.API;

public static class ApiDependencies
{
    private const int SessionTimeout = 1;
    
    public static void AddApiDependencies(this WebApplicationBuilder builder)
    {
        
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        builder.Services.Configure<AdminAuthSettings>(
            builder.Configuration.GetSection(nameof(AdminAuthSettings)));

        builder.Services.AddDistributedMemoryCache();
        
        var adminAuthSettings = builder
            .Configuration.GetSection(nameof(AdminAuthSettings)).Get<AdminAuthSettings>();
        
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(adminAuthSettings!.SessionTimeoutInMinutes);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = nameof(AdminSession);
        });

        builder.Services.AddSerilog(
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());

        builder.Services.AddControllersWithViews(o =>
        {
            o.Filters.Add(new MvcExceptionFilter());
        }); 
        
    }

    public static void UseApiDependencies(this WebApplication app)
    {
        app.MapOpenApi();

        app.MapScalarApiReference("/docs", options =>
        {
            options.Title = "Translator API";
            options.Theme = ScalarTheme.Mars;
            options.WithOpenApiRoutePattern("/openapi/v1.json");
        });
        app.MapControllers();
        app.UseHttpsRedirection();

        app.UseSession();

        app.UseMiddleware<ErrorHandlingMiddleware>();
        
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}");
    }
}