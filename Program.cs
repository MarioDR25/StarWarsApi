using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using StarWarsApi.Data;
using StarWarsApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StarWarsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StarWarsDbContext>();
    db.Database.EnsureCreated();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "An internal server error occurred." });
    });
});

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api/users").MapUserEndpoints();
app.MapGroup("/api/characters").MapCharacterEndpoints();
app.MapGroup("/api/planets").MapPlanetEndpoints();

app.Run();
