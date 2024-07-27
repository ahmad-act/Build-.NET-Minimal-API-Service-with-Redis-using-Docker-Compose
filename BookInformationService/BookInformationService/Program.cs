using Asp.Versioning;
using Asp.Versioning.Builder;
using Asp.Versioning.Conventions;
using BookInformationService;
using BookInformationService.DatabaseContext;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenApi;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using BookInformationService.BookInformation.List;
using BookInformationService.BookInformation.Get;
using BookInformationService.BookInformation.Create;
using BookInformationService.BookInformation.Update;
using BookInformationService.BookInformation.Delete;
using BookInformationService.BookInformation.Facade.List;
using BookInformationService.BookInformation.Facade.Get;
using BookInformationService.BookInformation.Facade.Create;
using BookInformationService.BookInformation.Facade.Update;
using BookInformationService.BookInformation.Facade.Delete;
using System;

//namespace BookInformationService;

var configuration = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json")
         .Build();

var defaultConnectionString = configuration.GetConnectionString("DefaultConnection");


AppSettings? appSettings = configuration.GetRequiredSection("AppSettings").Get<AppSettings>();


Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

// Configure Serilog with ASP.NET Core
builder.Host.UseSerilog(); // Serilog.AspNetCore


// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<SwaggerDefaultValues>();
});

builder.Services.AddApiVersioning(option =>
{
    option.DefaultApiVersion = new ApiVersion(1);
    option.AssumeDefaultVersionWhenUnspecified = true;
    option.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = (context) =>
    {
        context.ProblemDetails.Extensions["Service"] = "BookInformationService";
    };
});

// EF
// Configure PostgreSQL context
builder.Services.AddDbContext<SystemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register services for Dependency Injection (DI)
// List
builder.Services.AddScoped<IListBookInformationDL, ListBookInformationDL>();
builder.Services.AddScoped<IListBookInformationBL, ListBookInformationBL>();
// Get
builder.Services.AddScoped<IGetBookInformationDL, GetBookInformationDL>();
builder.Services.AddScoped<IGetBookInformationBL, GetBookInformationBL>();
// Create
builder.Services.AddScoped<ICreateBookInformationDL, CreateBookInformationDL>();
builder.Services.AddScoped<ICreateBookInformationBL, CreateBookInformationBL>();
// Update
builder.Services.AddScoped<IUpdateBookInformationDL, UpdateBookInformationDL>();
builder.Services.AddScoped<IUpdateBookInformationBL, UpdateBookInformationBL>();
// Delete
builder.Services.AddScoped<IDeleteBookInformationDL, DeleteBookInformationDL>();
builder.Services.AddScoped<IDeleteBookInformationBL, DeleteBookInformationBL>();


// Configure Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = configuration.GetConnectionString("Redis");
    options.InstanceName = "BookInformationServiceInstance:";
});


var app = builder.Build();

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(1)
    .HasApiVersion(2)
    .ReportApiVersions()
    .Build();

RouteGroupBuilder routeGroupBuilder = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

routeGroupBuilder.MapListBookInformationEndpoint();
routeGroupBuilder.MapGetBookInformationEndpoint();
routeGroupBuilder.MapCreateBookInformationEndpoint();
routeGroupBuilder.MapUpdateBookInformationEndpoint();
routeGroupBuilder.MapDeleteBookInformationEndpoint();


if (app.Environment.IsDevelopment())
{
    // Apply migrations at startup
    app.ApplyMigrations();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });

    // Redirect root URL to Swagger UI
    app.Use(async (context, next) =>
    {
        if (context.Request.Path == "/")
        {
            context.Response.Redirect("/swagger/index.html");
            return;
        }
        await next();
    });
}
else {
    app.UseHttpsRedirection();
}


app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature is not null)
        {
            Console.WriteLine($"Error: {contextFeature.Error}");
            Log.Fatal(contextFeature.Error, "Unhandled exception occurred.");
            Log.CloseAndFlush();

            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error"
            });
        }
    });
});

// Handle graceful shutdown
//app.Lifetime.ApplicationStopping.Register(async () =>
//{
//    // Write a message or perform actions during shutdown
//    Log.Information("Application is shutting down...");

//    // Optionally, you can use HttpContext to write response or perform other tasks
//    // But note that HttpContext might not be available here
//});

app.Run();

//app.Run(async (context) =>
//{
//    // Handle application shutdown gracefully
//    await context.Response.WriteAsync("Shutting down...");
//});
