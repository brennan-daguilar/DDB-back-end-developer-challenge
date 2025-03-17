using System.Text.Json;
using CharacterHitpointService.Models;

namespace CharacterHitpointService.CharacterService;

public class MockCharacterService : ICharacterService
{
    private readonly Dictionary<string, Character> _characters = new();

    public MockCharacterService()
    {
        LoadMockDataAsync("briv.json").Wait();
    }

    public async Task LoadMockDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        var character = JsonSerializer.Deserialize<Character>(json) ??
                        throw new NullReferenceException("Failed to deserialize character data.");
        _characters.Add(character.Name, character);
    }

    public Task<Character?> GetCharacterAsync(string characterId)
    {
        if (_characters.TryGetValue(characterId, out var character))
        {
            return Task.FromResult<Character?>(character);
        }

        return Task.FromResult<Character?>(null)!;
    }
}