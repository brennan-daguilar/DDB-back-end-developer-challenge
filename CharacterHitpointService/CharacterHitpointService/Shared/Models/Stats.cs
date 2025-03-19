namespace CharacterHitpointService.Shared.Models;

public class Stats
{
    public int Id { get; set; }
    public string CharacterId { get; set; }
    public int Strength { get; set; }
    public int Dexterity { get; set; }
    public int Constitution { get; set; }
    public int Intelligence { get; set; }
    public int Wisdom { get; set; }
    public int Charisma { get; set; }
}