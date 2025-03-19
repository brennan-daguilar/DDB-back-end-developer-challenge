using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CharacterHitpointService.Api.AddTemporaryHitpoints;
using CharacterHitpointService.Hitpoints.Models;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Test.Hitpoints.Api;

[Collection("ApiCollection")]
public class AddTemporaryHitpointsTests : BaseFunctionalTest
{
    public AddTemporaryHitpointsTests(TestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task AddTemporaryHitpoints_WithValidCharacterIdAndHitpointsValue_ShouldReturnOk()
    {
        // Arrange
        var characterId = "briv";
        var request = new AddTemporaryHitpointsRequest()
        {
            Amount = 10
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/addTemporaryHitpoints", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AddTemporaryHitpointsResult>();
        result.ShouldNotBeNull();
        result.CharacterId.ShouldBe("briv");
        result.Before.Hitpoints.ShouldBe(25);
        result.Before.TemporaryHitpoints.ShouldBe(0);
        result.After.Hitpoints.ShouldBe(25);
        result.After.TemporaryHitpoints.ShouldBe(10);
        result.Gained.ShouldBe(10);
    }

    [Fact]
    public async Task AddTemporaryHitpoints_WithUnknownCharacter_ShouldReturnBadRequest()
    {
        // Arrange
        var characterId = "notbriv";
        var request = new AddTemporaryHitpointsRequest()
        {
            Amount = 10
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/addTemporaryHitpoints", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        result.ShouldNotBeNull();
        result.Detail.ShouldBe("Failed to retrieve or create character health state.");
    }

    [Fact]
    public async Task AddTemporaryHitpoints_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var characterId = "briv";
        var request = new AddTemporaryHitpointsRequest()
        {
            Amount = -5
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/addTemporaryHitpoints", request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        result.ShouldNotBeNull();
        result.Extensions.ShouldContainKey("errors");
        var errors = ((JsonElement)result.Extensions["errors"]).Deserialize<Dictionary<string, string[]>>();
        errors.ShouldContainKey("Amount");
    }
}