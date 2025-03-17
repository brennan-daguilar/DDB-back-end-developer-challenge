using System.ComponentModel.DataAnnotations;

namespace CharacterHitpointService.Hitpoints;

public class CharacterHealthState
{
    [Key]
    public required string CharacterId { get; set; }
    public required int Hitpoints { get; set; }
    public required int TemporaryHitpoints { get; set; }
    public required int MaxHitpoints { get; set; }
}