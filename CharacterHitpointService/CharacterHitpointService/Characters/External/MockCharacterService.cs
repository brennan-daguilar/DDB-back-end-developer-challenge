using System.Text.Json;
using System.Text.Json.Serialization;
using CharacterHitpointService.Shared.Models;

namespace CharacterHitpointService.Characters.External;

public class MockCharacterService : ICharacterService
{
    private readonly Dictionary<string, Character> _characters = new();

    public MockCharacterService()
    {
        LoadMockDataAsync("briv.json").Wait();
    }

    public async Task LoadMockDataAsync(string filePath)
    {
        var options =
            new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        var json = await File.ReadAllTextAsync(filePath);
        var character = JsonSerializer.Deserialize<Character>(json, options) ??
                        throw new NullReferenceException("Failed to deserialize character data.");
        _characters.Add("briv", character);
    }

    public async Task<Character?> GetCharacterAsync(string characterId)
    {
        // Simulate latency from an external service
        await Task.Delay(500);

        if (_characters.TryGetValue(characterId, out var character))
        {
            return character;
        }

        return null;
    }
}