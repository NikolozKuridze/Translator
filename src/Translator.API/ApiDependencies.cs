using System.Net;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Scalar.AspNetCore;
using Serilog;
using Translator.API.Attributes;
using Translator.API.Middlewares;
using Translator.API.Models;
using Translator.API.Services;
using Translator.Application.Contracts;
using Translator.Infrastructure.Database.Postgres;

namespace Translator.API;

public static class ApiDependencies
{
    public static void AddApiDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddControllers();

        builder.Services.Configure<AdminAuthSettings>(
            builder.Configuration.GetSection(nameof(AdminAuthSettings)));

        builder.Services.Configure<ProductionUrl>(
            builder.Configuration.GetSection(nameof(ProductionUrl)));

        builder.Services.AddDistributedMemoryCache();

        var adminAuthSettings = builder
            .Configuration.GetSection(nameof(AdminAuthSettings))
            .Get<AdminAuthSettings>();

        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.Cookie.Name = nameof(AdminSession);
        });

        builder.Services.AddSerilog(
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger());

        builder.Services.AddScoped<MvcExceptionFilter>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

        builder.Services.AddControllersWithViews(o => { o.Filters.Add<MvcExceptionFilter>(); });

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
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logsDbContext = scope.ServiceProvider.GetRequiredService<LogsDbContext>();
            var creator = db.Database.GetService<IRelationalDatabaseCreator>();
            var logsCreator = logsDbContext.Database.GetService<IRelationalDatabaseCreator>();

            if (!db.Database.CanConnect())
                creator.Create();

            if (!logsDbContext.Database.CanConnect())
                logsCreator.Create();
        }

        var productionUrl = app.Configuration
            .GetSection(nameof(ProductionUrl))
            .Get<ProductionUrl>();

        app.MapOpenApi();

        var servers = new List<ScalarServer> { new(productionUrl!.Path) };

        app.MapScalarApiReference("/docs", options =>
        {
            options.Title = "Translator API";
            options.Theme = ScalarTheme.Mars;
            options.WithOpenApiRoutePattern("/openapi/v1.json");
            options.Servers = servers;
        });

        app.MapControllers();
        app.UseHttpsRedirection();

        app.UseSession();

        app.UseMiddleware<SecretKeyAuthenticationMiddleware>();

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.MapControllerRoute(
            "default",
            "{controller=Home}/{action=Index}");
    }
}