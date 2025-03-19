using CharacterHitpointService.Shared.Models;

namespace CharacterHitpointService.Characters;

public interface ICharacterRepository
{
    Task<Character?> GetCharacterAsync(string characterId);
}