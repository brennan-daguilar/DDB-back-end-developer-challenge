namespace CharacterHitpointService.Shared.Models;

public class ItemModifier
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string AffectedObject { get; set; }
    public string AffectedValue { get; set; }
    public int Value { get; set; }
}