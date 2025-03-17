using CharacterHitpointService.Models;

namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterResponse
{
    public required string CharacterId { get; set; }
    public required CombinedHitpoints Before { get; set; }
    public required CombinedHitpoints After { get; set; }
    public required int TotalDamage { get; set; }
    public DamageResistanceEffect? ResistanceEffect { get; set; }
}