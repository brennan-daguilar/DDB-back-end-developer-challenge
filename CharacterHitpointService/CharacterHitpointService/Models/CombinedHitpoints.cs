namespace CharacterHitpointService.Models;

public record CombinedHitpoints
{
    public int Hitpoints { get; set; }
    public int TemporaryHitpoints { get; set; }

    public CombinedHitpoints(int hitpoints, int temporaryHitpoints)
    {
        Hitpoints = hitpoints;
        TemporaryHitpoints = temporaryHitpoints;
    }
}