using FluentValidation;

namespace CharacterHitpointService.Api.Heal;

public class HealCharacterRequest
{
    public string CharacterId { get; set; }
    public int Value { get; set; }

    public sealed class Validator : AbstractValidator<HealCharacterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Value).GreaterThan(0);
        }
    }
}