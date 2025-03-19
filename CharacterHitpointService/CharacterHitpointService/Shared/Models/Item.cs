namespace CharacterHitpointService.Shared.Models;

public class Item
{
    public int Id { get; set; }
    public IEnumerable<Character> Characters;
    public string Name { get; set; }

    public ItemModifier? Modifier { get; set; }
}