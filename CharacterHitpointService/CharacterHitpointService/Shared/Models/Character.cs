namespace CharacterHitpointService.Shared.Models;

public class Character
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public int Hitpoints { get; set; }
    public IEnumerable<CharacterClass> Classes { get; set; } = null!;
    public Stats Stats { get; set; } = null!;
    public IEnumerable<Item> Items { get; set; } = null!;
    public IEnumerable<CharacterDefense> Defenses { get; set; } = null!;
}