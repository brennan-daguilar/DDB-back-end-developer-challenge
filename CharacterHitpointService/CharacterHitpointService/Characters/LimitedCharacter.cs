using CharacterHitpointService.Shared.Models;

namespace CharacterHitpointService.Characters;

public class LimitedCharacter
{
    public int Hitpoints { get; set; }
    public HashSet<CharacterDefense> Defenses { get; set; } = null!;
}