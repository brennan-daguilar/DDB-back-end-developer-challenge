using CharacterHitpointService.Models;

namespace CharacterHitpointService.Api.AddTemporaryHitpoints;

public class AddTemporaryHitpointsResponse
{
    public required string CharacterId { get; set; }
    public required CombinedHitpoints Before { get; set; }
    public required CombinedHitpoints After { get; set; }
    public required int Gained { get; set; }
}