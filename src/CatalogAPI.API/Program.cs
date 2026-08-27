using CatalogAPI.API.Configuration;
using CatalogAPI.Infrastructure.DependencyInjection;
using CatalogAPI.API.Endpoints;
using CatalogAPI.API.Extensions;
using CatalogAPI.Application.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.AddConfigureJsonStringEnumConverter();

builder.AddBearerAuthentication();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfig();

var app = builder.Build();

await app.MigrateAsync();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
    app.UseSwaggerConfig();

app.UseHttpsRedirection();

app.MapGame(); 
app.MapLibrary();

app.Run();

public partial class Program { }
