using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using Translator.API.Contracts;
using Translator.API.Middlewares;
using Translator.Application;
using Translator.Infrastructure;
using Translator.Infrastructure.Database.Postgres;
using Translator.Infrastructure.Database.Postgres.SeedData;

const int sessionTimeout = 1;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddApplicationDependencies();

builder.Services.AddOpenApi();
builder.Services.AddControllers();

builder.Services.Configure<AdminAuthSettings>(
    builder.Configuration.GetSection(nameof(AdminAuthSettings)));

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(sessionTimeout);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "AdminSession";
});

builder.Services.AddSerilog(
    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .CreateLogger());

builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add(new MvcExceptionFilter());
}); 

var app = builder.Build();

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


if (args.Contains("--seed"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seeder = new DatabaseSeeder(context);
        
    await context.Database.MigrateAsync();
    await context.Database.EnsureCreatedAsync();
        
    await seeder.SeedTranslationsAsync();
}


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}");

app.Run();
