using System.ComponentModel.DataAnnotations;

namespace CharacterHitpointService.Hitpoints.Models;

public class CharacterHitpointState
{
    [Key]
    public required string CharacterId { get; set; }
    public required int Hitpoints { get; set; }
    public required int TemporaryHitpoints { get; set; }
}