
namespace CharacterHitpointService.Characters;

public interface ICharacterRepository
{
    Task<LimitedCharacter?> GetCharacterAsync(string characterId);
}