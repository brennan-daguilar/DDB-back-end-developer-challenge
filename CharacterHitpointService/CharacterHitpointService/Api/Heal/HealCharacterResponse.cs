using CharacterHitpointService.Models;

namespace CharacterHitpointService.Api.Heal;

public class HealCharacterResponse
{
    public required string CharacterId { get; set; }
    public required CombinedHitpoints Before { get; set; }
    public required CombinedHitpoints After { get; set; }
    public required int ActualHealed { get; set; }
}