using Translator.API;
using Translator.Application;
using Translator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructureDependencies(builder.Configuration);
builder.Services.AddApplicationDependencies();
builder.AddApiDependencies();

var app = builder.Build();

app.UseApiDependencies();

app.Run();
