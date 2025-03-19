using CharacterHitpointService.Shared.Models;

namespace CharacterHitpointService.Characters.External;

public interface ICharacterService
{
    Task<Character?> GetCharacterAsync(string characterId);
}