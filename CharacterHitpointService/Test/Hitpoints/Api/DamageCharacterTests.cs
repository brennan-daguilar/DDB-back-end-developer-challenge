using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CharacterHitpointService.Api.Damage;
using CharacterHitpointService.Hitpoints.Models;
using CharacterHitpointService.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Test.Hitpoints.Api;

[Collection("ApiCollection")]
public class DamageCharacterTests : BaseFunctionalTest
{
    public DamageCharacterTests(TestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DamageCharacter_WithValidCharacterIdAndDamageValue_ShouldReturnOk()
    {
        // Arrange
        var characterId = "briv";
        var request = new DamageCharacterRequest()
        {
            Amount = 5,
            Type = "piercing"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/damage", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<DamageCharacterResult>();
        result.ShouldNotBeNull();
        result.CharacterId.ShouldBe("briv");
        result.Before.Hitpoints.ShouldBe(25);
        result.Before.TemporaryHitpoints.ShouldBe(0);
        result.After.Hitpoints.ShouldBe(20);
        result.After.TemporaryHitpoints.ShouldBe(0);
        result.TotalDamage.ShouldBe(5);
        result.ResistanceEffect.ShouldBeNull();
    }

    [Fact]
    public async Task DamageCharacter_WithUnknownCharacter_ShouldReturnBadRequest()
    {
        // Arrange
        var characterId = "notbriv";
        var request = new DamageCharacterRequest()
        {
            Amount = 5,
            Type = "piercing"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/damage", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        result.ShouldNotBeNull();
        result.Detail.ShouldBe("Character not found.");
    }

    [Fact]
    public async Task DamageCharacter_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var characterId = "notbriv";
        var request = new DamageCharacterRequest()
        {
            Amount = -1,
            Type = "magic"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/damage", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        result.ShouldNotBeNull();
        result.Extensions.ShouldContainKey("errors");
        var errors = ((JsonElement)result.Extensions["errors"]).Deserialize<Dictionary<string, string[]>>();
        errors.ShouldContainKey("Amount");
        errors.ShouldContainKey("Type");
    }
}