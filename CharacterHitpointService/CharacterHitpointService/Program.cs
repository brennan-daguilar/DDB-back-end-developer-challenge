using System.Text.Json.Serialization;
using CharacterHitpointService.Api;
using CharacterHitpointService.Characters;
using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Infrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddApiEndpoints();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddDbContext<HitpointsDbContext>(options => options.UseSqlite("Data Source=hitpoints.db"));
builder.Services.AddSingleton<ICharacterService, MockCharacterService>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<HitpointService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<HitpointsDbContext>();
    db.Database.EnsureCreated();
}

// app.UseHttpsRedirection();
app.MapApiEndpoints();
app.UseExceptionHandler();

app.Run();