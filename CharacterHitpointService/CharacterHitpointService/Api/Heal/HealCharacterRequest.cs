using FluentValidation;

namespace CharacterHitpointService.Api.Heal;

public class HealCharacterRequest
{
    public string CharacterId { get; set; }
    public int Amount { get; set; }

    public sealed class Validator : AbstractValidator<HealCharacterRequest>
    {
        public Validator()
        {
            RuleFor(x => x.CharacterId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}