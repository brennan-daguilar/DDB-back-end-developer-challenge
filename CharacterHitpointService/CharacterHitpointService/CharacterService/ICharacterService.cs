using CharacterHitpointService.Models;

namespace CharacterHitpointService.CharacterService;

public interface ICharacterService
{
    Task<Character?> GetCharacterAsync(string characterId);
}