using System.Text.Json;
using System.Text.Json.Serialization;
using CharacterHitpointService.Api;
using CharacterHitpointService.Characters;
using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Hitpoints;
using CharacterHitpointService.Infrastructure;
using CharacterHitpointService.Shared.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddDbContext<CharacterDbContext>(options => options.UseSqlite("Data Source=characters.db"));
builder.Services.AddScoped<ICharacterService, MockCharacterService>();
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
    var hpDb = services.GetRequiredService<HitpointsDbContext>();
    hpDb.Database.EnsureCreated();

    var characterDb = services.GetRequiredService<CharacterDbContext>();
    if (characterDb.Database.EnsureCreated())
    {
        var options =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        var json = await File.ReadAllTextAsync("briv.json");
        var character = JsonSerializer.Deserialize<Character>(json, options) ??
                        throw new NullReferenceException("Failed to deserialize character data.");
        character.Id = "briv";
        characterDb.Characters.Add(character);
        characterDb.SaveChanges();
    }
}

// app.UseHttpsRedirection();
app.MapApiEndpoints();
app.UseExceptionHandler();

app.Run();

public partial class Program;