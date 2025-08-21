using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Scalar.AspNetCore;
using Serilog;
using Translator.API.Attributes;
using Translator.API.Middlewares;
using Translator.API.Models;

namespace Translator.API;

public static class ApiDependencies
{
    
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
        
        builder.Services.AddScoped<MvcExceptionFilter>();
        
        builder.Services.AddControllersWithViews(o =>
        {
            o.Filters.Add<MvcExceptionFilter>();
        });

        builder.Services.Configure<CORSPolicy>(
            builder.Configuration.GetSection(nameof(CORSPolicy)));
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("IPWhitelistPolicy", policy =>
            {
                var corsSettings = builder.Configuration.GetSection(nameof(CORSPolicy)).Get<CORSPolicy>();
                if (corsSettings is null)
                    return;
                
                policy.SetIsOriginAllowed(origin =>
                    {
                        if (string.IsNullOrEmpty(origin)) 
                            return false;
                        try
                        {
                            var host = new Uri(origin).Host;
                            var hostIPs = Dns.GetHostAddresses(host);
                            var whitelistIPs = corsSettings.AllowedIPs.Select(IPAddress.Parse).ToList();
                
                            return hostIPs.Any(ip => whitelistIPs.Contains(ip));
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
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