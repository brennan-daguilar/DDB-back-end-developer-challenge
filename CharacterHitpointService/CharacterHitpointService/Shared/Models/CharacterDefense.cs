namespace CharacterHitpointService.Shared.Models;

public class CharacterDefense
{
    public int Id { get; set; }
    public string CharacterId { get; set; }
    public DamageType Type { get; set; }
    public DefenseValue Defense { get; set; }
}