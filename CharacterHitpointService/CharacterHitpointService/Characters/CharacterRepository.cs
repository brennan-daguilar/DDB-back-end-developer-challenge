using System.Text.Json;
using CharacterHitpointService.Characters.External;
using CharacterHitpointService.Hitpoints.Models;
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


    public async Task<LimitedCharacter?> GetCharacterAsync(string characterId)
    {
        var cacheKey = $"character:{characterId}";
        var cachedCharacter = await _cache.GetStringAsync(cacheKey);
        if (cachedCharacter != null)
        {
            try
            {
                return JsonSerializer.Deserialize<LimitedCharacter>(cachedCharacter);
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to deserialize cached character data. {CacheKey}", cacheKey);
            }
        }

        var character = await _characterService.GetCharacterAsync(characterId);
        if (character == null) 
            return null;
        
        var limitedCharacter = new LimitedCharacter()
        {
            Hitpoints = character.Hitpoints,
            Defenses = character.Defenses
        };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(limitedCharacter),
            new DistributedCacheEntryOptions()
            {
                SlidingExpiration = TimeSpan.FromMinutes(5)
            });

        return limitedCharacter;

    }
}