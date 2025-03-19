namespace CharacterHitpointService.Hitpoints.Models;

public class HealCharacterResult
{
    public required string CharacterId { get; set; }
    public required CombinedHitpoints Before { get; set; }
    public required CombinedHitpoints After { get; set; }
    public required int Healed { get; set; }
}