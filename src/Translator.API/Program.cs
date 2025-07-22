using Translator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructureDependencies(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();

app.UseHttpsRedirection();

app.Run();
