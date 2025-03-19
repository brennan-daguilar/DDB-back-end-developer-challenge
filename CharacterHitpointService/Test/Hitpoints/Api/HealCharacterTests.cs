using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CharacterHitpointService.Api.Heal;
using CharacterHitpointService.Hitpoints.Models;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace Test.Hitpoints.Api;

[Collection("ApiCollection")]
public class HealCharacterTests : BaseFunctionalTest
{
    public HealCharacterTests(TestWebAppFactory factory) : base(factory)
    {
    }
    
   [Fact]
       public async Task HealCharacter_WithValidCharacterIdAndHealValue_ShouldReturnOk()
       {
           // Arrange
           var characterId = "briv";
           var request = new HealCharacterRequest()
           {
               Amount = 5
           };
   
           // Act
           var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/heal", request);
   
           // Assert
           response.StatusCode.ShouldBe(HttpStatusCode.OK);
           var result = await response.Content.ReadFromJsonAsync<HealCharacterResult>();
           result.ShouldNotBeNull();
           result.CharacterId.ShouldBe("briv");
           result.Before.Hitpoints.ShouldBe(25);
           result.Before.TemporaryHitpoints.ShouldBe(0);
           result.After.Hitpoints.ShouldBe(25);
           result.After.TemporaryHitpoints.ShouldBe(0);
           result.Healed.ShouldBe(0);
       }
   
       [Fact]
       public async Task HealCharacter_WithUnknownCharacter_ShouldReturnBadRequest()
       {
           // Arrange
           var characterId = "notbriv";
           var request = new HealCharacterRequest()
           {
               Amount = 5
           };
   
           // Act
           var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/heal", request);
   
           // Assert
           response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
           var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
           result.ShouldNotBeNull();
           result.Detail.ShouldBe("Character not found.");
       }
   
       [Fact]
       public async Task HealCharacter_WithInvalidRequest_ShouldReturnBadRequest()
       {
           // Arrange
           var characterId = "briv";
           var request = new HealCharacterRequest()
           {
               Amount = -1
           };
   
           // Act
           var response = await HttpClient.PostAsJsonAsync($"/character/{characterId}/heal", request);
   
           // Assert
           response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
           var result = await response.Content.ReadFromJsonAsync<ProblemDetails>();
           result.ShouldNotBeNull();
           result.Extensions.ShouldContainKey("errors");
           var errors = ((JsonElement)result.Extensions["errors"]).Deserialize<Dictionary<string, string[]>>();
           errors.ShouldContainKey("Amount");
       } 
}