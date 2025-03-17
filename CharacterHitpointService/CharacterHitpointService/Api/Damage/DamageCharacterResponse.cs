namespace CharacterHitpointService.Api.Damage;

public class DamageCharacterResponse
{
    public required string CharacterId { get; set; }
    public required int Hp { get; set; }
    public required int TemporaryHp { get; set; }
}