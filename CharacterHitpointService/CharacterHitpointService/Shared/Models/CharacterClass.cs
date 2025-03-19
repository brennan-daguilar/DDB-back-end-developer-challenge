namespace CharacterHitpointService.Shared.Models;

public class CharacterClass
{
    public int Id { get; set; }
    public string CharacterId { get; set; }
    public string Name { get; set; }
    public int HitDiceValue { get; set; }
    public int ClassLevel { get; set; }
}