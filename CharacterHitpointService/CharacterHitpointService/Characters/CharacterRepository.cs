using System.Text.Json;
using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Shared.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace CharacterHitpointService.Characters;

public class CharacterRepository : ICharacterRepository
{
    private readonly ICharacterService _characterService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(ICharacterService characterService, IDistributedCache cache,
        ILogger<CharacterRepository> logger)
    {
        _characterService = characterService;
        _cache = cache;
        _logger = logger;
    }


    public async Task<Character?> GetCharacterAsync(string characterId)
    {
        var cacheKey = $"character:{characterId}";
        var cachedCharacter = await _cache.GetStringAsync(cacheKey);
        if (cachedCharacter != null)
        {
            try
            {
                return JsonSerializer.Deserialize<Character>(cachedCharacter);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to deserialize cached character data. {CacheKey}", cacheKey);
            }
        }

        _logger.LogWarning("Cache miss for character {CharacterId}", characterId);
        var character = await _characterService.GetCharacterAsync(characterId);
        if (character != null)
        {
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(character),
                new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
        }

        return character;
    }
}