namespace CharacterHitpointService.Shared.Models;

public class Character
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int Hitpoints { get; set; }
    public HashSet<CharacterClass> Classes { get; set; } = null!;
    public Stats Stats { get; set; } = null!;
    public HashSet<Item> Items { get; set; } = null!;
    public HashSet<CharacterDefense> Defenses { get; set; } = null!;
}